using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [Serializable]
    public class EnvironmentRotatingWheelConfig : IEquatable<ushort>
    {
        public ushort Id;
        public Vector2 CenterPosition;
        public float RotationSpeed;
        public WallConfig[] Walls;
        public WallConfig[] LavaWalls;
        public EnvironmentSpringConfig[] Springs;
        public EnvironmentSpikeConfig[] Spikes;

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
