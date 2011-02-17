using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Timers;
using Minecraft.Entities;
using Minecraft.Handlers;
using Minecraft.Map;
using Minecraft.Packet;
using Minecraft.Utilities;
using Minecraft.Items;

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
        public uint EID { get; set; }

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
                    throw new NullReferenceException();
                }

            }
            catch (NullReferenceException)
            {
                Disconnected();
            }
            catch
            {
                Log.Warning("Client disconnected from {0}.", EndPoint);
                Player.Save();
                Dispose();
            }
        }

        private void OnSend(IAsyncResult result)
        {
            int length = Client.EndSend(result);
        }

        private void Disconnected()
        {
            Log.Info("Client disconnected from {0}.", EndPoint);
            Player.Save();
            Dispose();
        }

        private void OnDisconnect(IAsyncResult result)
        {
            if (Client != null && Client.Connected)
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

                    if (MinecraftServer.Instance.Players.ContainsKey(Username.ToLower()))
                    {
                        MinecraftServer.Instance.Players.Remove(Username.ToLower());
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
            Player = new Player(this, Username, EID);

            MinecraftServer.Instance.Players.Add(Username.ToLower(), Player);

            //SEND Beginning Chunks
            //NOTE: Dunno if this is correct.

            Player.Update();

            foreach (Item i in Player.Inventory.Values)
            {
                Client.Send(MinecraftPacketCreator.GetSetSlot(0, i.Slot, i.ID, i.Count, i.Uses));
            }

            Client.Send(MinecraftPacketCreator.GetSpawnPosition(MinecraftServer.Instance.SpawnX, MinecraftServer.Instance.SpawnY, MinecraftServer.Instance.SpawnZ));

            Client.Send(MinecraftPacketCreator.GetPositionLook(Player.X, Player.Y, Player.Z, Player.Rotation, Player.OnGround));

            Client.Send(MinecraftPacketCreator.GetTimeUpdate(MinecraftServer.Instance.Time));
        }
    }
}
