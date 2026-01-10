using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Editor.LevelEnvironment.Scripts
{
    public class EnvironmentGenerator : MonoBehaviour
    {
        private const int MIN_BOX2D_ID = 1;
        [SerializeField] private EnvironmentConfig _environmentConfig;
        [SerializeField] private List<PolygonPath2D> _walls;
        
        [Button]
        public void RefreshConfig(int index)
        {
            _walls = GetWalls();
            var wallsConfigs = new WallConfig[_walls.Count];

            for (int i = 0; i < _walls.Count; i++)
            {
                var wallGenerator = _walls[i];
                var wallConfig = new WallConfig((ushort) (i + MIN_BOX2D_ID), wallGenerator.GetPointsRelativeToObject().Select(x=>x.ToNumericsVector2()).ToArray());
                wallsConfigs[i] = wallConfig;
            }

            _environmentConfig.SetWalls(wallsConfigs, index);
        }

        private List<PolygonPath2D> GetWalls()
        {
            return new List<PolygonPath2D>(GetComponentsInChildren<PolygonPath2D>());
        }
    }
}
