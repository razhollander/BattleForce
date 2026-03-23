using System;
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
            Core.Game.Domains.GamePlay.Shared.Extensions.NetDataWriterExtensions.PutVector2Quantized(writer, Position);
            Core.Game.Domains.GamePlay.Shared.Extensions.NetDataWriterExtensions.PutVector2Quantized(writer, Velocity);
            Core.Game.Domains.GamePlay.Shared.Extensions.NetDataWriterExtensions.PutFloat16(writer, Size);
            writer.Put((byte)CasterPlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            ProjectileId = reader.GetUShort();
            Position = Core.Game.Domains.GamePlay.Shared.Extensions.NetDataReaderExtensions.GetVector2Quantized(reader);
            Velocity = Core.Game.Domains.GamePlay.Shared.Extensions.NetDataReaderExtensions.GetVector2Quantized(reader);
            Size = Core.Game.Domains.GamePlay.Shared.Extensions.NetDataReaderExtensions.GetFloat16(reader);
            CasterPlayerId = reader.GetByte();
        }

        public int CompareTo(CreateKOProjectileNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
