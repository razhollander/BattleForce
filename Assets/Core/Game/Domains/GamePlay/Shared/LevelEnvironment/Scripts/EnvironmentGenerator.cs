using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.LevelEnvironment.Scripts
{
    public class EnvironmentGenerator : MonoBehaviour
    {
        [SerializeField] private EnvironmentConfig _environmentConfig;
        [SerializeField] private List<PolygonPath2D> _walls;
        [SerializeField] private List<LavaWall> _lavaWalls;
        [SerializeField] private List<PowerUpSpawnPoint> _powerUpSpawnPoints;
        [SerializeField] private SharedGamePlayConfig _sharedGamePlayConfig;

        [Button]
        public void RefreshConfig(int index)
        {
            // _walls = GetWalls();
            // var wallsConfigs = new WallConfig[_walls.Count];
            //
            // for (int i = 0; i < _walls.Count; i++)
            // {
            //     var wallGenerator = _walls[i];
            //     var wallConfig = new WallConfig((ushort) (i + _sharedGamePlayConfig.MinEntityId), wallGenerator.GetPointsRelativeToObject().Select(x=>x.ToNumericsVector2()).ToArray());
            //     wallsConfigs[i] = wallConfig;
            // }
            //
            // _environmentConfig.SetWalls(wallsConfigs, index);

            // _lavaWalls = GetLavaWalls();
            // var lavaConfigs = new WallConfig[_lavaWalls.Count];
            // var lavaStartId = _sharedGamePlayConfig.MinEntityId;
            //
            // for (int i = 0; i < _lavaWalls.Count; i++)
            // {
            //     var lavaWall = _lavaWalls[i];
            //     var lavaConfig = new WallConfig((ushort)(i + lavaStartId), lavaWall.GetPoints().Select(x => x.ToNumericsVector2()).ToArray());
            //     lavaConfigs[i] = lavaConfig;
            // }
            //
            // _environmentConfig.SetLavaWalls(lavaConfigs, index);
            
            var powerUpSpawnPointConfigs = new PowerUpSpawnPointConfig[_powerUpSpawnPoints.Count];

            for (int i = 0; i < _powerUpSpawnPoints.Count; i++)
            {
                powerUpSpawnPointConfigs[i] = new PowerUpSpawnPointConfig(_powerUpSpawnPoints[i].transform.position.ToVector2XY().ToNumericsVector2());
            }

            _environmentConfig.SetPowerUpSpawnPoints(powerUpSpawnPointConfigs, index);
        }

        private List<PolygonPath2D> GetWalls()
        {
            return GetComponentsInChildren<PolygonPath2D>().Where(p => p.GetComponent<LavaWall>() == null).ToList();
        }

        private List<LavaWall> GetLavaWalls()
        {
            return GetComponentsInChildren<LavaWall>().ToList();
        }

        private List<PowerUpSpawnPoint> GetPowerUpSpawnPoints()
        {
            return _powerUpSpawnPoints.ToList();
        }

        [Button]
        public void CreateWallsFromConfig(EnvironmentConfig layoutConfig, int index)
        {
            CreateWallPieces(layoutConfig.Configs[index].GetWalls(), false);
            CreateWallPieces(layoutConfig.Configs[index].GetLavaWalls(), true);
        }

        private void CreateWallPieces(WallConfig[] wallConfigs, bool isLava)
        {
            if (wallConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var wallConfig in wallConfigs)
            {
                var wallObject = new GameObject(isLava ? $"LavaWall_{wallConfig.Id}" : $"Wall_{wallConfig.Id}");
                wallObject.transform.SetParent(transform);
                wallObject.transform.position = Vector3.zero;

                var polygonPath = wallObject.AddComponent<PolygonPath2D>();
                polygonPath.Points.AddRange(wallConfig.Points.Select(p => p.ToUnityVector2()));

                if (isLava)
                {
                    wallObject.AddComponent<LavaWall>();
                }
            }
        }
    }
}
