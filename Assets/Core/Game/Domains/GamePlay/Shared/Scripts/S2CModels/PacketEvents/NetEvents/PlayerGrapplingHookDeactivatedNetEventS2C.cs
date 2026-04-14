using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerGrapplingHookDeactivatedNetEventS2C : INetSerializable, IComparable<PlayerGrapplingHookDeactivatedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort HookProjectileId;
        public ushort CasterPlayerId;
        public int TalentCooldownEndTick;

        public PlayerGrapplingHookDeactivatedNetEventS2C(int occuredOnTick, ushort hookProjectileId, ushort casterPlayerId, int talentCooldownEndTick)
        {
            OccuredOnTick = occuredOnTick;
            HookProjectileId = hookProjectileId;
            CasterPlayerId = casterPlayerId;
            TalentCooldownEndTick = talentCooldownEndTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(HookProjectileId);
            writer.Put((byte)CasterPlayerId);
            writer.Put(TalentCooldownEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            HookProjectileId = reader.GetUShort();
            CasterPlayerId = reader.GetByte();
            TalentCooldownEndTick = reader.GetInt();
        }

        public int CompareTo(PlayerGrapplingHookDeactivatedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
