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
        private const int FIRST_MOLE_HOLE_ID = 1; // zero is kept free so it can mean "no mole hole"

        [SerializeField] private EnvironmentConfig _environmentConfig;
        [SerializeField] private List<PolygonPath2D> _walls;
        [SerializeField] private List<LavaWall> _lavaWalls;
        [SerializeField] private List<PowerUpSpawnPoint> _powerUpSpawnPoints;
        [SerializeField] private List<MoleSpawnPoint> _moleSpawnPoints;
        [SerializeField] private List<ScoreGateSpawnPoint> _scoreGates;
        [SerializeField] private List<GateTrapSpawnPoint> _gateTraps;
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
                moleSpawnPointConfigs[i] = new MoleSpawnPointConfig((ushort)(i + FIRST_MOLE_HOLE_ID), _moleSpawnPoints[i].transform.position.ToVector2XY().ToNumericsVector2());
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

        // A trap that is not fully authored is reported by id and left out, so one unassigned reference cannot take the
        // whole bake down with a nameless NullReferenceException.
        [Button]
        public void RefreshGateTraps(int index)
        {
            var gateTrapConfigs = new List<EnvironmentGateTrapConfig>(_gateTraps.Count);

            foreach (var gateTrapSpawnPoint in _gateTraps)
            {
                var gateTrapConfig = BuildGateTrapConfig(gateTrapSpawnPoint);

                if (gateTrapConfig != null)
                {
                    gateTrapConfigs.Add(gateTrapConfig);
                }
            }

            _environmentConfig.SetGateTraps(gateTrapConfigs.ToArray(), index);
        }

        private EnvironmentGateTrapConfig BuildGateTrapConfig(GateTrapSpawnPoint gateTrapSpawnPoint)
        {
            if (!IsGateTrapFullyAuthored(gateTrapSpawnPoint))
            {
                return null;
            }

            var areaPolygons = new GateTrapAreaPolygonConfig[gateTrapSpawnPoint.AreaPolygons.Count];

            for (int i = 0; i < areaPolygons.Length; i++)
            {
                var areaPolygon = gateTrapSpawnPoint.AreaPolygons[i];
                var points = areaPolygon.GetPointsRelativeToObject().Select(p => p.ToNumericsVector2()).ToArray();
                WarnIfTooManyPoints(points.Length, $"GateTrap {gateTrapSpawnPoint.Id} area polygon {i}");
                areaPolygons[i] = new GateTrapAreaPolygonConfig { Points = points };
            }

            var wallPoints = gateTrapSpawnPoint.WallShape.GetPointsCCW().Select(p => p.ToNumericsVector2()).ToArray();
            WarnIfTooManyPoints(wallPoints.Length, $"GateTrap {gateTrapSpawnPoint.Id} wall");

            return new EnvironmentGateTrapConfig
            {
                Id = gateTrapSpawnPoint.Id,
                WallId = gateTrapSpawnPoint.WallId,
                WallPoints = wallPoints,
                AreaPolygons = areaPolygons,
                OpenPosition = gateTrapSpawnPoint.OpenPose.position.ToVector2XY().ToNumericsVector2(),
                ClosedPosition = gateTrapSpawnPoint.ClosedPose.position.ToVector2XY().ToNumericsVector2(),
                OpenRotationDegrees = gateTrapSpawnPoint.OpenPose.eulerAngles.z,
                ClosedRotationDegrees = gateTrapSpawnPoint.ClosedPose.eulerAngles.z,
                LocalRotationPivot = gateTrapSpawnPoint.LocalRotationPivot == null
                    ? System.Numerics.Vector2.Zero
                    : gateTrapSpawnPoint.LocalRotationPivot.localPosition.ToVector2XY().ToNumericsVector2(),
                MovementSpeed = gateTrapSpawnPoint.MovementSpeed,
                SecondsStayClosed = gateTrapSpawnPoint.SecondsStayClosed,
                SecondsStayOpen = gateTrapSpawnPoint.SecondsStayOpen,
                IsAttachedToRotationWheel = gateTrapSpawnPoint.IsAttachedToRotationWheel,
                AttachToRotationWheelId = gateTrapSpawnPoint.AttachToRotationWheelId
            };
        }

        private bool IsGateTrapFullyAuthored(GateTrapSpawnPoint gateTrapSpawnPoint)
        {
            if (gateTrapSpawnPoint == null)
            {
                Debug.LogError("The gate traps list has an empty slot, skipping it.");
                return false;
            }

            var missingReferenceName = GetMissingGateTrapReferenceName(gateTrapSpawnPoint);

            if (missingReferenceName == null)
            {
                return true;
            }

            Debug.LogError($"GateTrap {gateTrapSpawnPoint.Id} on '{gateTrapSpawnPoint.name}' has no {missingReferenceName} assigned, skipping it.", gateTrapSpawnPoint);
            return false;
        }

        private string GetMissingGateTrapReferenceName(GateTrapSpawnPoint gateTrapSpawnPoint)
        {
            if (gateTrapSpawnPoint.WallShape == null) return nameof(gateTrapSpawnPoint.WallShape);
            if (gateTrapSpawnPoint.OpenPose == null) return nameof(gateTrapSpawnPoint.OpenPose);
            if (gateTrapSpawnPoint.ClosedPose == null) return nameof(gateTrapSpawnPoint.ClosedPose);
            if (gateTrapSpawnPoint.AreaPolygons.IsNullOrEmpty()) return nameof(gateTrapSpawnPoint.AreaPolygons);

            foreach (var areaPolygon in gateTrapSpawnPoint.AreaPolygons)
            {
                if (areaPolygon == null) return nameof(gateTrapSpawnPoint.AreaPolygons) + " (it has an empty slot)";
            }

            return null;
        }

        // The wall becomes a Box2D polygon and the areas are tested against it, so both are capped at the same 8 points.
        private void WarnIfTooManyPoints(int pointsCount, string authoredObjectName)
        {
            if (pointsCount > GateTrapAreaPolygonConfig.MAX_POINTS)
            {
                Debug.LogError($"{authoredObjectName} has {pointsCount} points, only {GateTrapAreaPolygonConfig.MAX_POINTS} are supported!");
            }
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
