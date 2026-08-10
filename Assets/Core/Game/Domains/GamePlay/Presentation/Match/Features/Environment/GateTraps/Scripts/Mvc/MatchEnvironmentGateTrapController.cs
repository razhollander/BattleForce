using System;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.GateTraps.Scripts.Mvc
{
    /// <summary>
    /// Shows a gate trap's wall - the sensing area itself is never drawn. The wall greys out while the trap is cooling
    /// down and cannot catch anybody.
    /// </summary>
    public class MatchEnvironmentGateTrapController : IEquatable<ushort>
    {
        private static readonly Color ARMED_COLOR = Color.white;
        private static readonly Color ON_COOLDOWN_COLOR = new Color(0.42f, 0.42f, 0.42f, 1f);

        public readonly ushort GateTrapId;

        private readonly MatchEnvironmentGateTrapModel _gateTrapModel;
        private readonly float _exponentialDecay;

        private EnvironmentWallView _wallView;
        private Transform _wallViewTransform;
        private bool _isShownAsOnCooldown;

        public MatchEnvironmentGateTrapController(MatchEnvironmentGateTrapModel gateTrapModel, float exponentialDecay)
        {
            _gateTrapModel = gateTrapModel;
            _exponentialDecay = exponentialDecay;
            GateTrapId = gateTrapModel.Id;
        }

        public void CreateWallView(EnvironmentWallView wallViewPrefab, Transform parent)
        {
            _wallView = Object.Instantiate(wallViewPrefab, parent);
            _wallView.name = "EnvironmentGateTrapWall_" + GateTrapId;
            var pointsUnityVector2 = _gateTrapModel.WallPoints.Select(x => x.ToUnityVector2()).ToArray();
            _wallView.SetMesh(MeshUtils.BuildMesh(pointsUnityVector2, 0));
            _wallViewTransform = _wallView.transform;

            SetTransform(_gateTrapModel.WorldPosition.ToUnityVector2(), _gateTrapModel.WorldRotationAngle.AngleToQuaternion());
            ApplyCooldownColor(shouldForceApply: true);
        }

        public void UpdateView()
        {
            InterpolateTransform(_gateTrapModel.WorldPosition, _gateTrapModel.WorldRotationAngle);
            ApplyCooldownColor(shouldForceApply: false);
        }

        public void Destroy()
        {
            Object.Destroy(_wallView.gameObject);
        }

        public bool Equals(ushort otherId)
        {
            return GateTrapId == otherId;
        }

        private void InterpolateTransform(Vector2 position, float rotationDegrees)
        {
            var targetRotation = rotationDegrees.AngleToQuaternion();
            var deltaTime = Time.deltaTime;

            var interpolatedRotation = MathUtils.ExpDecay(_wallViewTransform.rotation, targetRotation, _exponentialDecay, deltaTime);
            var interpolatedPosition = MathUtils.ExpDecay(_wallViewTransform.position, position.ToUnityVector2(), _exponentialDecay, deltaTime);
            SetTransform(interpolatedPosition, interpolatedRotation);
        }

        private void ApplyCooldownColor(bool shouldForceApply)
        {
            var isOnCooldown = _gateTrapModel.IsWaitingForOpenCooldown;

            if (!shouldForceApply && isOnCooldown == _isShownAsOnCooldown)
            {
                return;
            }

            _isShownAsOnCooldown = isOnCooldown;
            _wallView.SetColor(isOnCooldown ? ON_COOLDOWN_COLOR : ARMED_COLOR);
        }

        private void SetTransform(UnityEngine.Vector2 position, Quaternion rotation)
        {
            _wallViewTransform.position = position;
            _wallViewTransform.rotation = rotation;
        }
    }
}
