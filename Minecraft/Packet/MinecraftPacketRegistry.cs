using Minecraft.Handlers;
using System.Collections.Generic;

namespace Minecraft.Packet
{
    class MinecraftPacketRegistry
    {
        private MinecraftPacketRegistry()
        {
            ReloadHandlers();
        }

        private Dictionary<byte, IPacketHandler> Handlers = new Dictionary<byte, IPacketHandler>();
        private static MinecraftPacketRegistry _Instance = new MinecraftPacketRegistry();

        public static MinecraftPacketRegistry Instance
        {
            get { return _Instance; }
            set { _Instance = value; }
        }

        public IPacketHandler GetHandler(byte id)
        {
            if (Handlers.ContainsKey(id))
            {
                return Handlers[id];
            }
            else
            {
                return null;
            }
        }

        public void ReloadHandlers()
        {
            Handlers.Clear();

            Handlers.Add((byte)MinecraftOpcode.KeepAlive, new KeepAliveHandler());
            Handlers.Add((byte)MinecraftOpcode.LoginRequest, new LoginRequestHandler());
            Handlers.Add((byte)HandshakePacketHandler.Opcode, new HandshakePacketHandler());
        }
    }
}
