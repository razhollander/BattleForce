#if UNITY_EDITOR

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
        [SerializeField] private List<MoleSpawnPoint> _moleSpawnPoints;
        [SerializeField] private List<ScoreGateSpawnPoint> _scoreGates;
        [SerializeField] private Transform _cameraTopLeftBoundary;
        [SerializeField] private Transform _cameraBottomRightBoundary;
        [SerializeField] private SharedGamePlayConfig _sharedGamePlayConfig;

        [Button]
        public void RefreshConfig(int index)
        {
            var powerUpSpawnPointConfigs = new PowerUpSpawnPointConfig[_powerUpSpawnPoints.Count];

            for (int i = 0; i < _powerUpSpawnPoints.Count; i++)
            {
                powerUpSpawnPointConfigs[i] = new PowerUpSpawnPointConfig(_powerUpSpawnPoints[i].transform.position.ToVector2XY().ToNumericsVector2());
            }

            _environmentConfig.SetPowerUpSpawnPoints(powerUpSpawnPointConfigs, index);
        }

        [Button]
        public void RefreshMoleSpawnPoints(int index)
        {
            var moleSpawnPointConfigs = new MoleSpawnPointConfig[_moleSpawnPoints.Count];

            for (int i = 0; i < _moleSpawnPoints.Count; i++)
            {
                moleSpawnPointConfigs[i] = new MoleSpawnPointConfig(_moleSpawnPoints[i].transform.position.ToVector2XY().ToNumericsVector2());
            }

            _environmentConfig.SetMoleSpawnPoints(moleSpawnPointConfigs, index);
        }

        [Button]
        public void RefreshScoreGates(int index)
        {
            var scoreGateConfigs = new ScoreGateConfig[_scoreGates.Count];

            for (int i = 0; i < _scoreGates.Count; i++)
            {
                scoreGateConfigs[i] = new ScoreGateConfig(
                    _scoreGates[i].Id,
                    _scoreGates[i].transform.position.ToVector2XY().ToNumericsVector2(),
                    _scoreGates[i].transform.eulerAngles.z);
            }

            _environmentConfig.SetScoreGates(scoreGateConfigs, index);
        }

        [Button]
        public void SaveCameraBoundaries(int index)
        {
            var topLeft = _cameraTopLeftBoundary.position.ToVector2XY().ToNumericsVector2();
            var bottomRight = _cameraBottomRightBoundary.position.ToVector2XY().ToNumericsVector2();
            _environmentConfig.SetCameraBoundaries(new CameraBoundariesConfig(topLeft, bottomRight), index);
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

        private void OnDrawGizmos()
        {
            if (_cameraTopLeftBoundary == null || _cameraBottomRightBoundary == null)
            {
                return;
            }

            var topLeft = _cameraTopLeftBoundary.position;
            var bottomRight = _cameraBottomRightBoundary.position;
            var center = (topLeft + bottomRight) * 0.5f;
            var size = new Vector3(Mathf.Abs(bottomRight.x - topLeft.x), Mathf.Abs(topLeft.y - bottomRight.y), 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
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
#endif
