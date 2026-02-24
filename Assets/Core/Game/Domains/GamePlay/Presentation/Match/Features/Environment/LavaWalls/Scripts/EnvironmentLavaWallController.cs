using System;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts
{
    public class EnvironmentLavaWallController : IEquatable<ushort>
    {
        private EnvironmentLavaWallView _lavaWallView;
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        public readonly ushort LavaWallId;
        private Transform _lavaWallViewTransform;

        public EnvironmentLavaWallController(ushort lavaWallId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            LavaWallId = lavaWallId;
        }
        
        public void CreateWallView(EnvironmentLavaWallView wallViewPrefab, Transform parent)
        {
            var lavaWallModel = _matchDataService.GetEnvironmentLavaWall(LavaWallId);
            _lavaWallView = Object.Instantiate(wallViewPrefab, parent);
            _lavaWallView.name = "EnvironmentLavaWall_" + LavaWallId;
            var pointsUnityVector2 = lavaWallModel.Points.Select(x => x.ToUnityVector2()).ToArray();
            var mesh = MeshUtils.BuildMesh(pointsUnityVector2, LayerOrder.EnvironmentWall);
            _lavaWallView.SetMesh(mesh);
            _lavaWallViewTransform = _lavaWallView.transform;
            SetTransform(lavaWallModel.WorldPosition.ToUnityVector2(), lavaWallModel.WorldRotationAngle.AngleToQuaternion());
        }

        public bool Equals(ushort otherId)
        {
            return LavaWallId == otherId;
        }

        public void InterpulateTransform(Vector2 position, float rotationDegrees)
        {
            var direction = rotationDegrees.ToRadians().AngleToVector();
            var targetRotation = direction.ToQuaternion();
            var deltaTime = Time.deltaTime;
            var decay = _gamePlayConfig.ExponentialDecay;
            
            var interpulatedRotation = MathUtils.ExpDecay(
                _lavaWallViewTransform.rotation, 
                targetRotation, 
                decay,
                deltaTime
            );
            
            var interpulatedPosition = MathUtils.ExpDecay(_lavaWallViewTransform.position, position.ToUnityVector2(), decay, deltaTime);
            SetTransform(interpulatedPosition, interpulatedRotation);
        }

        private void SetTransform(UnityEngine.Vector2 position, Quaternion rotation)
        {
            _lavaWallViewTransform.position = position;
            _lavaWallViewTransform.rotation = rotation;
        }
        
        public void Destroy()
        {
            Object.Destroy(_lavaWallView.gameObject);
        }
    }
}