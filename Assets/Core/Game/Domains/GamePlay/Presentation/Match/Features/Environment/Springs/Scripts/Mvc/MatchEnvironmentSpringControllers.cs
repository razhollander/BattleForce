using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public class MatchEnvironmentSpringControllers : IMatchEnvironmentSpringControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly MatchEnvironmentSpringView _environmentSpringViewPrefab;
        private readonly Dictionary<ushort, MatchEnvironmentSpringController> _springControllers = new Dictionary<ushort, MatchEnvironmentSpringController>();
        private GameObject _springsParent;

        public MatchEnvironmentSpringControllers(IMatchDataService matchDataService, MatchEnvironmentSpringView environmentSpringViewPrefab)
        {
            _matchDataService = matchDataService;
            _environmentSpringViewPrefab = environmentSpringViewPrefab;
        }

        public void InitEntryPoint()
        {
            _springsParent = new GameObject("EnvironmentSpringsParent");
        }

        public void CreateSpring(ushort springId)
        {
            var springModel = _matchDataService.GetEnvironmentSpring(springId);
            if (springModel == null) return;

            var springController = new MatchEnvironmentSpringController(springModel);
            springController.CreateView(_environmentSpringViewPrefab, _springsParent.transform);
            _springControllers.Add(springId, springController);
        }

        public MatchEnvironmentSpringController GetSpring(ushort springId)
        {
            if (_springControllers.TryGetValue(springId, out var controller))
            {
                return controller;
            }
            return null;
        }

        public void DestroyAll()
        {
            foreach (var controller in _springControllers.Values)
            {
                controller.Destroy();
            }
            _springControllers.Clear();
            if (_springsParent != null)
            {
                Object.Destroy(_springsParent);
                _springsParent = null;
            }
        }
    }
}
