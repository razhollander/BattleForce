using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateMagneticPullFieldNetEventS2C : INetSerializable, IComparable<CreateMagneticPullFieldNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public Vector2 Direction;
        public int TalentCooldownEndTick;
        public ushort HitEnemyId;

        public CreateMagneticPullFieldNetEventS2C(int occuredOnTick, ushort casterPlayerId, Vector2 direction, int talentCooldownEndTick, ushort hitEnemyId)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            Direction = direction;
            TalentCooldownEndTick = talentCooldownEndTick;
            HitEnemyId = hitEnemyId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(CasterPlayerId);
            writer.PutVector2AsAngle16(Direction);
            writer.Put(TalentCooldownEndTick);
            writer.Put(HitEnemyId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetUShort();
            Direction = reader.GetVector2FromAngle16();
            TalentCooldownEndTick = reader.GetInt();
            HitEnemyId = reader.GetUShort();
        }

        public int CompareTo(CreateMagneticPullFieldNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
