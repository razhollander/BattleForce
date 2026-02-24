using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate
{
    public class EnvironmentTeleportGatePairController
    {
        public ushort TeleportPairId { get; private set; }
        
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private EnvironmentTeleportGateView _gateAView;
        private EnvironmentTeleportGateView _gateBView;

        public EnvironmentTeleportGatePairController(ushort teleportPairId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig)
        {
            TeleportPairId = teleportPairId;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
        }

        public void CreateGateViews(EnvironmentTeleportGateView teleportGateView, Transform parent)
        {
            var teleportPairModel = _matchDataService.GetTeleportPair(TeleportPairId);
            
            _gateAView = CreateGateView(teleportGateView, parent, teleportPairModel.GateA);
            _gateBView = CreateGateView(teleportGateView, parent, teleportPairModel.GateB);

            if (_gamePlayConfig.Teleports.TeleportSpritesPerId.TryGetValue(TeleportPairId, out var teleportSprite))
            {
                _gateAView.Setup(teleportSprite, teleportPairModel.Size.ToUnityVector2());
                _gateBView.Setup(teleportSprite, teleportPairModel.Size.ToUnityVector2());
            }
            else
            {
                LogService.LogError("No teleport sprite found for teleport pair id: " + TeleportPairId);
            }
        }

        private EnvironmentTeleportGateView CreateGateView(EnvironmentTeleportGateView teleportGateViewPrefab, Transform parent, MatchEnvironmentTeleportGateModel teleportGateModel)
        {
            var teleportGateView = Object.Instantiate(teleportGateViewPrefab, parent);
            SetTransform(teleportGateView.transform, teleportGateModel.WorldPosition.ToUnityVector2(), teleportGateModel.WorldRotation.AngleToQuaternion());
            return teleportGateView;
        }

        public void InterpulateGatesTransforms()
        {
            var teleportPairModel = _matchDataService.GetTeleportPair(TeleportPairId);
            InterpulateTransform(_gateAView.transform, teleportPairModel.GateA.WorldPosition.ToUnityVector2(), teleportPairModel.GateA.WorldRotation);
            InterpulateTransform(_gateBView.transform, teleportPairModel.GateB.WorldPosition.ToUnityVector2(), teleportPairModel.GateB.WorldRotation);
        }
        
        private void InterpulateTransform(Transform transform, Vector2 position, float rotationDegrees)
        {
            var direction = rotationDegrees.ToRadians().AngleToVector();
            var targetRotation = direction.ToQuaternion();
            var deltaTime = Time.deltaTime;
            var decay = _gamePlayConfig.ExponentialDecay;
            
            var interpulatedRotation = MathUtils.ExpDecay(
                transform.rotation, 
                targetRotation, 
                decay,
                deltaTime
            );
            
            var interpulatedPosition = MathUtils.ExpDecay(transform.position, position, decay, deltaTime);
            SetTransform(transform, interpulatedPosition, interpulatedRotation);
        }
        
        private void SetTransform(Transform transform, Vector2 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        public void PlayTeleportAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _gateAView.PlayBounceAnimation(cancellationTokenSource).Forget();
            _gateBView.PlayBounceAnimation(cancellationTokenSource).Forget();
        }
        
        public void Destroy()
        {
            _gateAView.Destroy();
            _gateBView.Destroy();
        }
    }
}
