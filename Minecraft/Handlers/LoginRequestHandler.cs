using Minecraft.Net;
using Minecraft.Packet;
using System.Net;
using System.Text;

namespace Minecraft.Handlers
{
    class LoginRequestHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 4)
            {
                int version = stream.ReadInt();
                if (version == MinecraftServer.Instance.Version)
                {
                    if (stream.Length - stream.Position >= 2)
                    {
                        short ulength = stream.ReadShort();
                        if (stream.Length - stream.Position >= ulength)
                        {
                            string username = stream.ReadString(ulength);
                            if (stream.Length - stream.Position >= 2)
                            {
                                short plength = stream.ReadShort();
                                if (stream.Length - stream.Position >= plength)
                                {
                                    string password = stream.ReadString(plength);
                                    if (stream.Length - stream.Position >= 8)
                                    {
                                        stream.ReadLong();
                                        if (stream.Length - stream.Position >= 1)
                                        {
                                            stream.ReadByte();

                                            if (MinecraftServer.Instance.Authentication == MinecraftAuthentication.Online)
                                            {
                                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.minecraft.net/game/checkserver.jsp?user=" + username + "&serverId=" + client.Hash);
                                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                                byte[] buffer = new byte[3];
                                                response.GetResponseStream().Read(buffer, 0, 3);
                                                if (UTF8Encoding.UTF8.GetString(buffer) == "YES")
                                                {
                                                    client.Send(MinecraftPacketCreator.GetLoginRequest());
                                                    client.LoadConfiguration();
                                                    return true;
                                                }
                                                else
                                                {
                                                    client.Disconnect("Failed to verify username.");
                                                }
                                            }
                                            else if (MinecraftServer.Instance.Authentication == MinecraftAuthentication.Offline)
                                            {
                                                client.Send(MinecraftPacketCreator.GetLoginRequest());
                                                client.LoadConfiguration();
                                                return true;
                                            }
                                            else if (MinecraftServer.Instance.Authentication == MinecraftAuthentication.Password)
                                            {
                                                // Insert validation function here...
                                                // Password doesn't even work.
                                                client.Disconnect("Password authentication not implemented.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    client.Disconnect("Invalid version.");
                }
            }
            return false;
        }
    }
}
