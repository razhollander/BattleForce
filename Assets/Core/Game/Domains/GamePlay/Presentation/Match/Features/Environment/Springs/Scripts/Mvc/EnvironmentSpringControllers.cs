using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public class EnvironmentSpringControllers : IEnvironmentSpringControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentSpringView _environmentSpringViewPrefab;
        private readonly IStateMachineService _stateMachineService;
        private readonly Dictionary<ushort, MatchEnvironmentSpringController> _springControllers = new Dictionary<ushort, MatchEnvironmentSpringController>();
        private GameObject _springsParent;

        public EnvironmentSpringControllers(IMatchDataService matchDataService, EnvironmentSpringView environmentSpringViewPrefab, IStateMachineService stateMachineService)
        {
            _matchDataService = matchDataService;
            _environmentSpringViewPrefab = environmentSpringViewPrefab;
            _stateMachineService = stateMachineService;
        }

        public void InitEntryPoint()
        {
            _springsParent = new GameObject("EnvironmentSpringsParent");
        }

        public void CreateSpring(ushort springId)
        {
            var springController = new MatchEnvironmentSpringController();
            var springModel = _matchDataService.GetEnvironmentSpring(springId);
            springController.CreateView(_environmentSpringViewPrefab, _springsParent.transform, springModel.Position, springModel.DirectionAngle);
            _springControllers.Add(springId, springController);
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
            _springControllers[springId].PlayBounceAnimation(_stateMachineService.CurrentState().CancellationTokenSource);
        }
    }
}
