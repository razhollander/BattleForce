using System;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchEnvironmentWallController : IEquatable<ushort>
    {
        private EnvironmentWallView _wallView;
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        public readonly ushort WallId;
        private Transform _wallViewTransform;

        public MatchEnvironmentWallController(ushort wallId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            WallId = wallId;
        }
        
        public void CreateWallView(EnvironmentWallView wallViewPrefab, Transform parent)
        {
            var wallModel = _matchDataService.GetEnvironmentWall(WallId);
            _wallView = Object.Instantiate(wallViewPrefab, parent);
            _wallView.name = "EnvironmentWall_" + WallId;
            var pointsUnityVector2 = wallModel.Points.Select(x => x.ToUnityVector2()).ToArray();
            var mesh = MeshUtils.BuildMesh(pointsUnityVector2, 0);
            _wallView.SetMesh(mesh);
            _wallViewTransform = _wallView.transform;
            SetTransform(wallModel.WorldPosition.ToUnityVector2(), wallModel.WorldRotationAngle.AngleToQuaternion());
        }

        public void Destroy()
        {
            Object.Destroy(_wallView.gameObject);
        }

        public bool Equals(ushort otherId)
        {
            return WallId == otherId;
        }

        public void InterpulateTransform(Vector2 position, float rotationDegrees)
        {
            var direction = rotationDegrees.ToRadians().AngleToVector();
            var targetRotation = direction.ToQuaternion();
            var deltaTime = Time.deltaTime;
            var decay = _gamePlayConfig.ExponentialDecay;
            
            var interpulatedRotation = MathUtils.ExpDecay(
                _wallViewTransform.rotation, 
                targetRotation, 
                decay,
                deltaTime
            );
            
            var interpulatedPosition = MathUtils.ExpDecay(_wallViewTransform.position, position.ToUnityVector2(), decay, deltaTime);
            SetTransform(interpulatedPosition, interpulatedRotation);
        }
        
        private void SetTransform(UnityEngine.Vector2 position, Quaternion rotation)
        {
            _wallViewTransform.position = position;
            _wallViewTransform.rotation = rotation;
        }
    }
}