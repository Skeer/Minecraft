using System.Net.Sockets;
using System;
using Minecraft.Utilities;
using System.IO;
using Minecraft.Packet;
using Minecraft.Handlers;
using System.Timers;
using NBTLibrary;
using System.Collections.Generic;
using Minecraft.Entities;
using Minecraft.Map;

namespace Minecraft.Net
{
    public class MinecraftClient : MarshalByRefObject, IDisposable
    {
        private static Logger Log = new Logger(typeof(MinecraftClient));
        private bool Disposed = false;
        private byte[] Buffer = new byte[1500];
        private Socket Client;
        private string EndPoint;
        private MinecraftPacketStream Received = new MinecraftPacketStream();
        private Timer KeepAliveTimer = new Timer(30000);
        private Timer ConnectionTimer = new Timer(60000);

        public string Username { get; set; }
        public string Hash { get; set; }
        public Player Player { get; set; }

        public MinecraftClient(Socket client)
        {
            Client = client;
            EndPoint = client.RemoteEndPoint.ToString();

            KeepAliveTimer.Elapsed += new ElapsedEventHandler(KeepAliveTimer_Elapsed);
            KeepAliveTimer.Start();

            ConnectionTimer.Elapsed += new ElapsedEventHandler(ConnectionTimer_Elapsed);
            ConnectionTimer.Start();

            // TODO: Research ReceiveAsync again...
            Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
        }

        private void ConnectionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Warning("Client supposedly timed out. I don't know if this works, hence disabled.");
            //Disconnect("Client not responding.");
        }

        public void Disconnect(string message)
        {
            Client.Send(MinecraftPacketCreator.GetDisconnect(message));
            Client.BeginDisconnect(false, OnDisconnect, null);
        }

        private void KeepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(MinecraftPacketCreator.GetKeepAlive());
        }

        public void Send(byte[] buffer)
        {
            Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, OnSend, null);
        }

        private void ProcessReceived()
        {
            while (true)
            {
                if (Received.Length - Received.Position > 0)
                {
                    long position = Received.Position;
                    byte id = Received.ReadByte();

                    IPacketHandler handler = MinecraftServer.Instance.PacketRegistry.GetHandler(id);

                    if (handler == null)
                    {
                        Log.Warning("Unable to process packet with id {0}.", id);
                        Disconnect("Server failed to process packet.");
                        Dispose();
                    }
                    else
                    {
                        if (!handler.HandlePacket(this, Received))
                        {
                            Received.Position = position;
                            break;
                        }
                    }
                }
                else
                {
                    Received.SetLength(0);
                    break;
                }
            }
        }

        private void OnReceive(IAsyncResult result)
        {
            try
            {
                int length = Client.EndReceive(result);
                if (length > 0)
                {
                    long position = Received.Position;
                    Received.Position = Received.Length;
                    Received.Write(Buffer, 0, length);
                    Received.Position = position;

                    ProcessReceived();

                    Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
                }
                else
                {
                    Log.Info("Client disconnected from {0}.", EndPoint);
                    Dispose();
                }

            }
            catch (NullReferenceException)
            {
                Log.Info("Client disconnected from {0}.", EndPoint);
                Dispose();
            }
            catch (Exception e)
            {
                Log.Error(e, "Client disconnected from {0}.", EndPoint);
                Dispose();
            }
        }

        private void OnSend(IAsyncResult result)
        {
            int length = Client.EndSend(result);
        }

        private void OnDisconnect(IAsyncResult result)
        {
            if (Client != null)
            {
                Client.EndDisconnect(result);
            }
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (KeepAliveTimer != null)
                    {
                        KeepAliveTimer.Dispose();
                    }

                    if (ConnectionTimer != null)
                    {
                        ConnectionTimer.Dispose();
                    }

                    if (Client != null)
                    {
                        Client.Dispose();
                    }

                    if (Received != null)
                    {
                        Received.Dispose();
                    }

                    if(MinecraftServer.Instance.Clients.Contains(this))
                    {
                        MinecraftServer.Instance.Clients.Remove(this);
                    }

                    //Remove client for Server's client list?
                }

                Received = null;
                KeepAliveTimer = null;
                ConnectionTimer = null;
                Client = null;
                Disposed = true;
            }
        }

        ~MinecraftClient()
        {
            Dispose(false);
        }

        public void ResetConnectionTimer()
        {
            ConnectionTimer.Stop();
            ConnectionTimer.Start();
        }

        public void Load()
        {
            Player = new Player(Username);

            //SEND Beginning Chunks
            //NOTE: Dunno if this is correct.
            List<Chunk> chunks = MinecraftServer.Instance.ChunkManager.GetChunks(new PointInt() { X = (int)Player.Position.X / 16, Z = (int)Player.Position.Z / 16 });
            foreach (Chunk c in chunks)
            {
                //TODO: Probably be better to not pass the whole chunk........
                Client.Send(MinecraftPacketCreator.GetPreChunk(c));
                Client.Send(MinecraftPacketCreator.GetMapChunk(c));
            }

            Client.Send(MinecraftPacketCreator.GetSpawnPosition(MinecraftServer.Instance.SpawnPosition));

            Client.Send(MinecraftPacketCreator.GetPositionLook(Player.Position, Player.Rotation, Player.OnGround));
        }
    }
}
