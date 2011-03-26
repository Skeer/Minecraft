using System.Collections.Generic;
using Minecraft.Handlers;

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
            Handlers.Add((byte)MinecraftOpcode.HoldingChange, new HoldingChangeHandler());
            Handlers.Add((byte)MinecraftOpcode.PlayerDigging, new PlayerDiggingHandler());
            Handlers.Add((byte)MinecraftOpcode.Animation, new AnimationHandler());
            Handlers.Add((byte)MinecraftOpcode.EntityAction, new EntityActionHandler());
            Handlers.Add((byte)MinecraftOpcode.PlayerBlockPlacement, new PlayerBlockPlacementHandler());
            Handlers.Add((byte)MinecraftOpcode.Player, new PlayerHandler());
            Handlers.Add((byte)MinecraftOpcode.PlayerPosition, new PlayerPositionHandler());
            Handlers.Add((byte)MinecraftOpcode.PlayerLook, new PlayerLookHandler());
            Handlers.Add((byte)MinecraftOpcode.PlayerPositionLook, new PlayerPositionLookHandler());
            Handlers.Add((byte)MinecraftOpcode.Disconnect, new DisconnectPacketHandler());
        }
    }
}
