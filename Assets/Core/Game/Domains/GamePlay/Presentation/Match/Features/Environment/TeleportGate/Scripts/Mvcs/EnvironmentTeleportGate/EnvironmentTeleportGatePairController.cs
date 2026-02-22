using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
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
            teleportGateView.transform.position = teleportGateModel.Position.ToUnityVector2();
            teleportGateView.transform.rotation = teleportGateModel.NormalRotation.AngleToQuaternion();
            return teleportGateView;
        }

        public void PlayTeleportAnimation()
        {
            _gateAView.PlayTeleportAnimation();
            _gateBView.PlayTeleportAnimation();
        }
        
        public void Destroy()
        {
            _gateAView.Destroy();
            _gateBView.Destroy();
        }
    }
}
