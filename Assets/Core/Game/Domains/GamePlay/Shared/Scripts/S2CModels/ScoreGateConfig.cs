using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public class ScoreGateConfig
    {
        public ushort Id;
        public Vector2 Position;
        public float RotationDegrees;

        public ScoreGateConfig() {}

        public ScoreGateConfig(ushort id, Vector2 position, float rotationDegrees)
        {
            Id = id;
            Position = position;
            RotationDegrees = rotationDegrees;
        }
    }
}
