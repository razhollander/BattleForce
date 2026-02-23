using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public class EnvironmentSpringConfig : IEquatable<ushort>
    {
        public ushort Id;
        public Vector2 Position;
        private float DirectionAngle;

        public float RotationAngle
        {
            get { return DirectionAngle + 90; }
        }

        public EnvironmentSpringConfig(ushort id, Vector2 position, float directionAngle)
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
