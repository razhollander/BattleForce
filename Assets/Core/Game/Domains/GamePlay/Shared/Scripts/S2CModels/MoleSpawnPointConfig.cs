using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public class MoleSpawnPointConfig
    {
        public Vector2 Position;

        public MoleSpawnPointConfig() {}

        public MoleSpawnPointConfig(Vector2 position)
        {
            Position = position;
        }
    }
}
