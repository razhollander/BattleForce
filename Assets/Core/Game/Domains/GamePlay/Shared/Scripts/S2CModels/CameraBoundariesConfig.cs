using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public class CameraBoundariesConfig
    {
        public Vector2 TopLeft;
        public Vector2 BottomRight;

        public CameraBoundariesConfig() {}

        public CameraBoundariesConfig(Vector2 topLeft, Vector2 bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }
    }
}
