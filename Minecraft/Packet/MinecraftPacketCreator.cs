using Minecraft.Net;
namespace Minecraft.Packet
{
    class MinecraftPacketCreator
    {
        public static byte[] GetKeepAlive()
        {
            return new byte[] { (byte)MinecraftOpcode.KeepAlive };
        }

        public static byte[] GetDisconnect(string message)
        {
            MinecraftPacketStream stream = new MinecraftPacketStream();
            stream.WriteByte((byte)MinecraftOpcode.Disconnect);
            stream.WriteString(message);
            return stream.GetBytes();
        }

        public static byte[] GetHandshake(string hash)
        {
            MinecraftPacketStream stream = new MinecraftPacketStream();
            stream.WriteByte((byte)MinecraftOpcode.Handshake);
            stream.WriteString(hash);
            return stream.GetBytes();
        }

        public static byte[] GetLoginRequest()
        {
            MinecraftPacketStream stream = new MinecraftPacketStream();
            stream.WriteByte((byte)MinecraftOpcode.LoginRequest);
            stream.WriteUint(MinecraftServer.Instance.Entity++);
            stream.WriteString("");
            stream.WriteString("");
            stream.WriteLong(MinecraftServer.Instance.RandomSeed);
            stream.WriteByte(MinecraftServer.Instance.Dimension);
            return stream.GetBytes();
        }
    }
}
