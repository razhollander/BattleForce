using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [Serializable]
    public class EnvironmentRotatingWheelConfig
    {
        public ushort Id;
        public Vector2 CenterPosition;
        public float RotationSpeed;
        public WallConfig[] Walls;
        public WallConfig[] LavaWalls;
        public EnvironmentSpringConfig[] Springs;
    }
}
