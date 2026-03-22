using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateKOTalentNetEventS2C : INetSerializable, IComparable<DeactivateKOTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort KoProjectileId;
        public ushort CasterPlayerId;

        public DeactivateKOTalentNetEventS2C(int occuredOnTick, ushort koProjectileId, ushort casterPlayerId)
        {
            OccuredOnTick = occuredOnTick;
            KoProjectileId = koProjectileId;
            CasterPlayerId = casterPlayerId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(KoProjectileId);
            writer.Put((byte)CasterPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            KoProjectileId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
        }

        public int CompareTo(DeactivateKOTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
