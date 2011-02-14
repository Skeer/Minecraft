using Minecraft.Handlers;
using System.Collections.Generic;

namespace Minecraft.Packet
{
    public class MinecraftPacketRegistry
    {
        private Dictionary<byte, IPacketHandler> Handlers = new Dictionary<byte, IPacketHandler>();

        public MinecraftPacketRegistry()
        {
            ReloadHandlers();
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
            Handlers.Add((byte)MinecraftOpcode.Handshake, new HandshakePacketHandler());
            Handlers.Add((byte)MinecraftOpcode.ChatMessage, new ChatMessageHandler());
            Handlers.Add((byte)MinecraftOpcode.PlayerPosition, new PlayerPositionHandler());
            Handlers.Add((byte)MinecraftOpcode.PlayerPositionLook, new PlayerPositionLookHandler());
        }
    }
}
