using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateKOTalentNetEventS2C : INetSerializable, IComparable<DeactivateKOTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ProjectileId;
        public ushort CasterPlayerId;
        public int CooldownEndTick;

        public DeactivateKOTalentNetEventS2C(int occuredOnTick, ushort koProjectileId, ushort casterPlayerId, int cooldownEndTick)
        {
            OccuredOnTick = occuredOnTick;
            ProjectileId = koProjectileId;
            CasterPlayerId = casterPlayerId;
            CooldownEndTick = cooldownEndTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(ProjectileId);
            writer.Put((byte)CasterPlayerId);
            writer.Put(CooldownEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            CooldownEndTick = reader.GetInt();
        }

        public int CompareTo(DeactivateKOTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
