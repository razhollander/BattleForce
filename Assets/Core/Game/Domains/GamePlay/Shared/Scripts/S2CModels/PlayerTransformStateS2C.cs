using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerTransformStateS2C : INetSerializable
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Vector2 Direction;
        public float Radius;
        public float AngularVelocity;
        public Vector2 AimVector;

        public Vector2 GetHeadPosition()
        {
            return Position + Direction * Radius;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Position);
            // writer.Put(Velocity);
            // writer.Put(Acceleration);
            writer.Put(Direction);
            writer.Put(Radius);
            // writer.Put(AngularVelocity);
            // writer.Put(AimVector);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector2();
            // Velocity = reader.GetVector2();
            // Acceleration = reader.GetVector2();
            Direction = reader.GetVector2();
            Radius = reader.GetFloat();
            // AngularVelocity = reader.GetFloat();
            // AimVector = reader.GetVector2();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.Put(Position);
            writer.Put(Direction);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Position = reader.GetVector2();
            Direction = reader.GetVector2();
        }
    }
}