using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Editor.LevelEnvironment.Scripts
{
    public class EnvironmentGenerator : MonoBehaviour
    {
        private const int MIN_BOX2D_ID = 1;
        [SerializeField] private EnvironmentConfig _environmentConfig;
        private List<WallGenerator> _walls;

        [Button]
        public void RefreshConfig()
        {
            _walls = GetWalls();
            var wallsConfigs = new WallConfig[_walls.Count];

            for (int i = 0; i < _walls.Count; i++)
            {
                var wallGenerator = _walls[i];
                var wallConfig = new WallConfig((ushort) (i + MIN_BOX2D_ID), wallGenerator.GetPoints().ToArray());
                wallsConfigs[i] = wallConfig;
            }

            _environmentConfig.SetWalls(wallsConfigs);
        }

        private List<WallGenerator> GetWalls()
        {
            return new List<WallGenerator>(GetComponentsInChildren<WallGenerator>());
        }
    }
}
