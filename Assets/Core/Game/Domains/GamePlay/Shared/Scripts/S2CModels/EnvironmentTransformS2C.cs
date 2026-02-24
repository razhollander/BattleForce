using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public struct EnvironmentTransformS2C
    {
        public Vector2 LocalPosition;
        public Vector2 WorldPosition;
        public float WorldRotationDegrees;
        public float LocalRotationDegrees;
    }
}