using System;
using LiteNetLib.Utils;
using Core.Game.Domains.GamePlay.Shared.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerMaxShootCooldownChangedNetEventS2C : INetSerializable, IComparable<PlayerMaxShootCooldownChangedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public float MaxShootCooldown;
        public float ShootCooldownSecondsLeft;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.PutFloat16(MaxShootCooldown);
            writer.PutFloat16(ShootCooldownSecondsLeft);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            MaxShootCooldown = reader.GetFloat16();
            ShootCooldownSecondsLeft = reader.GetFloat16();
        }

        public int CompareTo(PlayerMaxShootCooldownChangedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
