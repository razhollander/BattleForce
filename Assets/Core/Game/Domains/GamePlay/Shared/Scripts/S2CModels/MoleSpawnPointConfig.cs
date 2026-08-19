using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public class MoleSpawnPointConfig
    {
        public ushort MoleHoleId;
        public Vector2 Position;

        public MoleSpawnPointConfig() {}

        public MoleSpawnPointConfig(ushort moleHoleId, Vector2 position)
        {
            MoleHoleId = moleHoleId;
            Position = position;
        }
    }
}
