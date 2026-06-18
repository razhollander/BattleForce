using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc
{
    public class EnvironmentSpikeControllers : IEnvironmentSpikeControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentSpikeView _environmentSpikeViewPrefab;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly Dictionary<ushort, MatchEnvironmentSpikeController> _spikeControllers = new Dictionary<ushort, MatchEnvironmentSpikeController>();
        private GameObject _spikesParent;

        public EnvironmentSpikeControllers(IMatchDataService matchDataService, EnvironmentSpikeView environmentSpikeViewPrefab, IStageCancellationTokenProvider stageCancellationTokenProvider, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _environmentSpikeViewPrefab = environmentSpikeViewPrefab;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _spikesParent = new GameObject("EnvironmentSpikesParent");
        }

        public void CreateSpike(ushort spikeId)
        {
            var spikeController = new MatchEnvironmentSpikeController(_gamePlayConfig);
            var spikeModel = _matchDataService.GetEnvironmentSpike(spikeId);
            spikeController.CreateView(_environmentSpikeViewPrefab, _spikesParent.transform, spikeModel.WorldPosition.ToUnityVector2(), spikeModel.WorldRotationAngle);
            _spikeControllers.Add(spikeId, spikeController);
        }

        public void UpdateSpikeTransform(ushort spikeId)
        {
            var spikeModel = _matchDataService.GetEnvironmentSpike(spikeId);
            _spikeControllers[spikeId].InterpulateTransform(spikeModel.WorldPosition.ToUnityVector2(), spikeModel.WorldRotationAngle);
        }

        public void DestroyAll()
        {
            foreach (var controller in _spikeControllers.Values)
            {
                controller.Destroy();
            }
            _spikeControllers.Clear();
        }

        public void PlaySpikeHitAnimation(ushort spikeId)
        {
            _spikeControllers[spikeId].PlayHitAnimation(_stageCancellationTokenProvider.CancellationTokenSource);
        }
    }
}
