using System.Net.Sockets;
using System;
using Minecraft.Utilities;
using System.IO;
using Minecraft.Packet;
using Minecraft.Handlers;
using System.Timers;
using NBTLibrary;
using System.Collections.Generic;

namespace Minecraft.Net
{
    class MinecraftClient : IDisposable
    {
        private static Logger Log = new Logger(typeof(MinecraftClient));
        private byte[] Buffer = new byte[1500];
        private Socket Client;
        private string EndPoint;
        private MinecraftPacketStream Received = new MinecraftPacketStream();
        private Timer KeepAliveTimer = new Timer(30000);
        private Timer ConnectionTimer = new Timer(60000);
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }

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
            Disconnect("Client not responding.");
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

                    IPacketHandler handler = MinecraftPacketRegistry.Instance.GetHandler(id);

                    if (handler == null)
                    {
                        Log.Warning("Unable to process packet with id {0}.", id);
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
                    Log.Info("Received {0} bytes.", length);
                    // TODO: Check for stream issues


                    Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
                }
                else
                {
                    throw new Exception();
                }

            }
            catch
            {
                Log.Info("Client disconnected from {0}.", EndPoint);
                Dispose();
            }
        }

        private void OnSend(IAsyncResult result)
        {
            int length = Client.EndSend(result);

            Log.Info("Sent {0} bytes.", length);
        }

        private void OnDisconnect(IAsyncResult result)
        {
            Client.EndDisconnect(result);
            Dispose();
        }

        public void Dispose()
        {
            KeepAliveTimer.Dispose();
            ConnectionTimer.Dispose();
            Client.Dispose();
        }

        public string Username { get; set; }

        public void ResetConnectionTimer()
        {
            ConnectionTimer.Stop();
            ConnectionTimer.Start();
        }

        public string Hash { get; set; }

        public void LoadConfiguration()
        {
            using (NBTFile file = NBTFile.Open(MinecraftServer.Instance.Path + "Players/" + Username + ".dat"))
            {
                List<Tag> pos = (List<Tag>) file.FindPayload("Pos");
                X = (double)pos[0].Payload;
                Y = (double)pos[1].Payload;
                Z = (double)pos[2].Payload;

                List<Tag> rotation = (List<Tag>)file.FindPayload("Rotation");
                Yaw = (double)rotation[0].Payload;
                Pitch = (double)rotation[1].Payload;
            }
        }
    }
}
