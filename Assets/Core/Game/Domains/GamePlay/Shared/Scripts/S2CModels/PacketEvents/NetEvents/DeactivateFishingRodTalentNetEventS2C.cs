using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateFishingRodTalentNetEventS2C : INetSerializable, IComparable<DeactivateFishingRodTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ProjectileId;
        public ushort CasterPlayerId;
        public int TalentCooldownEndTick;

        public DeactivateFishingRodTalentNetEventS2C(int occuredOnTick, ushort projectileId, ushort casterPlayerId, int talentCooldownEndTick)
        {
            OccuredOnTick = occuredOnTick;
            ProjectileId = projectileId;
            CasterPlayerId = casterPlayerId;
            TalentCooldownEndTick = talentCooldownEndTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(ProjectileId);
            writer.Put((byte)CasterPlayerId);
            writer.Put(TalentCooldownEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            TalentCooldownEndTick = reader.GetInt();
        }

        public int CompareTo(DeactivateFishingRodTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
