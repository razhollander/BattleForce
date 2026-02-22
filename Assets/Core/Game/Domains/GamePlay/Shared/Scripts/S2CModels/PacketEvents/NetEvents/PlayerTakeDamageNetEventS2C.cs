using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerTakeDamageNetEventS2C : INetSerializable, IComparable<PlayerTakeDamageNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public ushort PlayerHealth;
        public ushort HitDamage;
        public bool IsAlive;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.Put((byte)PlayerHealth);
            writer.Put((byte)HitDamage);
            writer.Put(IsAlive);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            PlayerHealth = reader.GetByte();
            HitDamage = reader.GetByte();
            IsAlive = reader.GetBool();
        }

        public int CompareTo(PlayerTakeDamageNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}