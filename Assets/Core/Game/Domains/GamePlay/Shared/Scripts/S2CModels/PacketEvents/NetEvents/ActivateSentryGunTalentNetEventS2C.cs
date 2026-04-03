using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct ActivateSentryGunTalentNetEventS2C : INetSerializable, IComparable<ActivateSentryGunTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public float Duration;

        public ActivateSentryGunTalentNetEventS2C(int occuredOnTick, ushort casterPlayerId, float duration)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            Duration = duration;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put(Duration);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            Duration = reader.GetFloat();
        }

        public int CompareTo(ActivateSentryGunTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
