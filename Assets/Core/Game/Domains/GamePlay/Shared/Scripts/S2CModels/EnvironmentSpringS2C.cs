using System;
using System.Numerics;
using System.Runtime.Serialization;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public class EnvironmentSpringS2C : IEquatable<ushort>
    {
        public ushort Id;
        public Vector2 Position;
        public float DirectionAngle;

        public float RotationAngle
        {
            get { return DirectionAngle + 90; }
        }

        public float WorldRotationAngle
        {
            set { WorldDirectionAngle = value - 90; }
        }
        
        [NonSerialized]
        public float WorldDirectionAngle;
        
        [OnDeserialized]
        private void OnSerialize(StreamingContext context)
        {
            WorldRotationAngle = RotationAngle;
            LogService.LogError($"id: {Id}, rotationAngle: {RotationAngle}, WorldDirectionAngle: {WorldDirectionAngle}");
        }

        public EnvironmentSpringS2C(ushort id, Vector2 position, float directionAngle)
        {
            Id = id;
            Position = position;
            DirectionAngle = directionAngle;
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
