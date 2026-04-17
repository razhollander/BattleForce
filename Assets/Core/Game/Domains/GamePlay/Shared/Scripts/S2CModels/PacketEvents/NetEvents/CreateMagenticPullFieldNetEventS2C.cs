using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateMagenticPullFieldNetEventS2C : INetSerializable, IComparable<CreateMagenticPullFieldNetEventS2C>
    {
        public int OccuredOnTick;
        public Vector2 Position;
        public Vector2 Rotation;
        public bool HasHit;
        public ushort HitPlayerId;
        public ushort CasterPlayerId;

        public CreateMagenticPullFieldNetEventS2C(int occuredOnTick, Vector2 position, Vector2 rotation, bool hasHit, ushort hitPlayerId, ushort casterPlayerId)
        {
            OccuredOnTick = occuredOnTick;
            Position = position;
            Rotation = rotation;
            HasHit = hasHit;
            HitPlayerId = hitPlayerId;
            CasterPlayerId = casterPlayerId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Rotation);
            writer.Put(HasHit);
            if (HasHit)
            {
                writer.Put((byte)HitPlayerId);
            }
            writer.Put((byte)CasterPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            Position = reader.GetVector2Quantized();
            Rotation = reader.GetVector2FromAngle16();
            HasHit = reader.GetBool();
            if (HasHit)
            {
                HitPlayerId = reader.GetByte();
            }
            else
            {
                HitPlayerId = 0;
            }
            CasterPlayerId = reader.GetByte();
        }

        public int CompareTo(CreateMagenticPullFieldNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}