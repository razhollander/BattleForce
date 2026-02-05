using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerDiedNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public float PlayerMaxShootCooldown;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.PutFloat16(PlayerMaxShootCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            PlayerMaxShootCooldown = reader.GetFloat16();
        }
    }
}
