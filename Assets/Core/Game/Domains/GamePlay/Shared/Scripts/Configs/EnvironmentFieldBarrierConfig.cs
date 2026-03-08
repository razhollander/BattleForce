using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [Serializable]
    public class EnvironmentFieldBarrierConfig
    {
        public Vector2 Position;
        public Vector2 Size;
        public FieldBarrierShape Shape;

        public EnvironmentFieldBarrierConfig() {}

        public EnvironmentFieldBarrierConfig(Vector2 position, Vector2 size, FieldBarrierShape shape)
        {
            Position = position;
            Size = size;
            Shape = shape;
        }
    }
}
