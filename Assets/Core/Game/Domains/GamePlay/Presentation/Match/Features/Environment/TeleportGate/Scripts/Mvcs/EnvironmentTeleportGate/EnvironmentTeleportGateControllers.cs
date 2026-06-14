using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate
{
    public class EnvironmentTeleportGateControllers : IEnvironmentTeleportGateControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentTeleportGateView _prefab;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly DiContainer _container;
        private Transform _parent;
        private readonly List<EnvironmentTeleportGatePairController> _controllers = new List<EnvironmentTeleportGatePairController>();

        public EnvironmentTeleportGateControllers(IMatchDataService matchDataService, EnvironmentTeleportGateView teleportGateViewPrefab, IStageCancellationTokenProvider stageCancellationTokenProvider,
            PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _prefab = teleportGateViewPrefab;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _parent = new GameObject("EnvironmentTeleportGates").transform;
        }
        
        public EnvironmentTeleportGatePairController CreateGatePair(ushort pairId)
        {
            var controller = new EnvironmentTeleportGatePairController(pairId, _matchDataService, _gamePlayConfig);
            controller.CreateGateViews(_prefab, _parent);
            _controllers.Add(controller);
            return controller;
        }

        private EnvironmentTeleportGatePairController GetGate(ushort pairId)
        {
            return _controllers.Find(c => c.TeleportPairId == pairId);
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.Destroy();
            }
            _controllers.Clear();
        }

        public void PlayTeleportAnimation(ushort pairId)
        {
            GetGate(pairId).PlayTeleportAnimation(_stageCancellationTokenProvider.CancellationTokenSource);
        }

        public void UpdateTeleportGateTransform(ushort pairId, bool isGateA)
        {
            GetGate(pairId).InterpulateGatesTransforms(isGateA);
        }
    }
}
