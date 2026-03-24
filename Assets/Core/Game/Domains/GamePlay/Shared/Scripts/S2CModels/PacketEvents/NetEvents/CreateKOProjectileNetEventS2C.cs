using System;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateKOProjectileNetEventS2C : INetSerializable, IComparable<CreateKOProjectileNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort ProjectileId;
        public System.Numerics.Vector2 Position;
        public System.Numerics.Vector2 Velocity;
        public float Size;
        public ushort CasterPlayerId;

        public CreateKOProjectileNetEventS2C(int occuredOnTick, ushort projectileId, ushort casterPlayerId, System.Numerics.Vector2 position, System.Numerics.Vector2 velocity, float size)
        {
            OccuredOnTick = occuredOnTick;
            ProjectileId = projectileId;
            Position = position;
            Velocity = velocity;
            Size = size;
            CasterPlayerId = casterPlayerId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(ProjectileId);
            writer.PutVector2Quantized(Position);
            writer.PutVector2Quantized(Velocity);
            writer.PutFloat16(Size);
            writer.Put((byte)CasterPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            Position = reader.GetVector2Quantized();
            Velocity = reader.GetVector2Quantized();
            Size = reader.GetFloat16();
            CasterPlayerId = reader.GetByte();
        }

        public int CompareTo(CreateKOProjectileNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
