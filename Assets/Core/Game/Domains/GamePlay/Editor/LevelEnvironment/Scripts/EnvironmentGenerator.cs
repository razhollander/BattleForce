using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Editor.LevelEnvironment.Scripts
{
    public class EnvironmentGenerator : MonoBehaviour
    {
        [SerializeField] private EnvironmentConfig _environmentConfig;
        [SerializeField] private List<PolygonPath2D> _walls;
        [SerializeField] private List<LavaWall> _lavaWalls;
        [SerializeField] private SharedGamePlayConfig _sharedGamePlayConfig;
        
        [Button]
        public void RefreshConfig(int index)
        {
            _walls = GetWalls();
            var wallsConfigs = new WallConfig[_walls.Count];

            for (int i = 0; i < _walls.Count; i++)
            {
                var wallGenerator = _walls[i];
                var wallConfig = new WallConfig((ushort) (i + _sharedGamePlayConfig.MinEntityId), wallGenerator.GetPointsRelativeToObject().Select(x=>x.ToNumericsVector2()).ToArray());
                wallsConfigs[i] = wallConfig;
            }

            _environmentConfig.SetWalls(wallsConfigs, index);

            _lavaWalls = GetLavaWalls();
            var lavaConfigs = new WallConfig[_lavaWalls.Count];
            var lavaStartId = _sharedGamePlayConfig.MinEntityId;

            for (int i = 0; i < _lavaWalls.Count; i++)
            {
                var lavaWall = _lavaWalls[i];
                var lavaConfig = new WallConfig((ushort)(i + lavaStartId), lavaWall.GetPoints().Select(x => x.ToNumericsVector2()).ToArray());
                lavaConfigs[i] = lavaConfig;
            }

            _environmentConfig.SetLavaWalls(lavaConfigs, index);
        }

        private List<PolygonPath2D> GetWalls()
        {
            return GetComponentsInChildren<PolygonPath2D>().Where(p => p.GetComponent<LavaWall>() == null).ToList();
        }

        private List<LavaWall> GetLavaWalls()
        {
            return GetComponentsInChildren<LavaWall>().ToList();
        }
    }
}
