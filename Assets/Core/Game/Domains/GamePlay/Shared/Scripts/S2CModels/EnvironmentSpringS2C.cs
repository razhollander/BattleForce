using System;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class EnvironmentSpringS2C : IEquatable<ushort>
    {
        private const float RotationToDirectionDegrees = 90;
        
        public ushort Id;
        public EnvironmentTransformS2C Transform;
        
        public float WorldDirectionDegrees
        {
            get { return Transform.WorldRotationDegrees-RotationToDirectionDegrees; }
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}