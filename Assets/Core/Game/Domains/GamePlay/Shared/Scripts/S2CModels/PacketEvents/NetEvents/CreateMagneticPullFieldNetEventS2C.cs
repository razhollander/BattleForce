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
        public Vector2 Position;
        public Vector2 Direction;
        public int TalentCooldownEndTick;
        public bool HasHit;
        public ushort HitEnemyId;
        
        public CreateMagneticPullFieldNetEventS2C(int occuredOnTick, ushort casterPlayerId, Vector2 position, Vector2 direction, int talentCooldownEndTick, bool hasHit, ushort hitEnemyId)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            Position = position;
            Direction = direction;
            TalentCooldownEndTick = talentCooldownEndTick;
            HitEnemyId = hitEnemyId;
            HasHit = hasHit;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Direction);
            writer.Put(TalentCooldownEndTick);
            writer.Put((byte)HitEnemyId);
            writer.Put(HasHit);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            Position = reader.GetVector2Quantized();
            Direction = reader.GetVector2FromAngle16();
            TalentCooldownEndTick = reader.GetInt();
            HitEnemyId = reader.GetByte();
            HasHit = reader.GetBool();
        }

        public int CompareTo(CreateMagneticPullFieldNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
