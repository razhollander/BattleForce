using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerTransformStateS2C : INetSerializable
    {
        public Vector2 Position;

        public Vector2 Velocity
        {
            get { return _Velocity;}
            set
            {
                _Velocity = value;
            }
        }

        public void StopMotion()
        {
            _Velocity = Vector2.Zero;
            AngularVelocity = 0;
        }

        public Vector2 _Velocity;
        public Vector2 Direction;
        public float Radius;
        public float AngularVelocity;

        public Vector2 GetHeadPosition()
        {
            return Position + Direction * Radius;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Direction);
            writer.PutFloat16(Radius);
            writer.PutVector2Quantized(_Velocity);
            writer.PutFloat16(AngularVelocity);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector2Quantized();
            Direction = reader.GetVector2FromAngle16();
            Radius = reader.GetFloat16();
            _Velocity = reader.GetVector2Quantized();
            AngularVelocity = reader.GetFloat16();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.PutVector2Quantized(Position);
            writer.PutVector2AsAngle16(Direction);
            writer.PutVector2Quantized(_Velocity);
            writer.PutFloat16(AngularVelocity);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Position = reader.GetVector2Quantized();
            Direction = reader.GetVector2FromAngle16();
            _Velocity = reader.GetVector2Quantized();
            AngularVelocity = reader.GetFloat16();
        }
    }
}