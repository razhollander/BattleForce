using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public class EnvironmentSpringControllers : IEnvironmentSpringControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentSpringView _environmentSpringViewPrefab;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly Dictionary<ushort, MatchEnvironmentSpringController> _springControllers = new Dictionary<ushort, MatchEnvironmentSpringController>();
        private GameObject _springsParent;

        public EnvironmentSpringControllers(IMatchDataService matchDataService, EnvironmentSpringView environmentSpringViewPrefab, IStageCancellationTokenProvider stageCancellationTokenProvider, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _environmentSpringViewPrefab = environmentSpringViewPrefab;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _springsParent = new GameObject("EnvironmentSpringsParent");
        }

        public void CreateSpring(ushort springId)
        {
            var springController = new MatchEnvironmentSpringController(_gamePlayConfig);
            var springModel = _matchDataService.GetEnvironmentSpring(springId);
            springController.CreateView(_environmentSpringViewPrefab, _springsParent.transform, springModel.WorldPosition.ToUnityVector2(), springModel.WorldRotationAngle);
            _springControllers.Add(springId, springController);
        }

        public void UpdateSpringTransform(ushort springId)
        {
            var springModel = _matchDataService.GetEnvironmentSpring(springId);
            _springControllers[springId].InterpulateTransform(springModel.WorldPosition.ToUnityVector2(), springModel.WorldRotationAngle);
        }
        
        public void DestroyAll()
        {
            foreach (var controller in _springControllers.Values)
            {
                controller.Destroy();
            }
            _springControllers.Clear();
        }

        public void PlaySpringBounceAnimation(ushort springId)
        {
            _springControllers[springId].PlayBounceAnimation(_stageCancellationTokenProvider.CancellationTokenSource);
        }
    }
}
