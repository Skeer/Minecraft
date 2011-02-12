namespace Minecraft.Packet
{
    enum MinecraftOpcode
    {
        KeepAlive = 0x00,
        LoginRequest = 0x01,
        Handshake = 0x02,
        Disconnect = 0xff
    }
}
