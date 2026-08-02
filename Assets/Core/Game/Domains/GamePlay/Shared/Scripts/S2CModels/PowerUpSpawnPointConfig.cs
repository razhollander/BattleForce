using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public class PowerUpSpawnPointConfig
    {
        public Vector2 Position;

        public PowerUpSpawnPointConfig() {}

        public PowerUpSpawnPointConfig(Vector2 position)
        {
            Position = position;
        }
    }
}
