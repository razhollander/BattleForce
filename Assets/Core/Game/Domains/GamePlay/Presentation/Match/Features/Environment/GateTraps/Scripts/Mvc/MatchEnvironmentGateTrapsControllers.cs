using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.GateTraps.Scripts.Mvc
{
    public class MatchEnvironmentGateTrapsControllers : IMatchEnvironmentGateTrapsControllers
    {
        private const string PARENT_GAME_OBJECT_NAME = "EnvironmentGateTrapsParent";

        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentWallView _wallViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchEnvironmentGateTrapController> _gateTrapControllers = new();
        private GameObject _gateTrapsParent;

        public MatchEnvironmentGateTrapsControllers(IMatchDataService matchDataService, EnvironmentWallView wallViewPrefab, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _wallViewPrefab = wallViewPrefab;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _gateTrapsParent = new GameObject(PARENT_GAME_OBJECT_NAME);
        }

        public void CreateGateTrap(ushort gateTrapId)
        {
            var gateTrapModel = _matchDataService.GetGateTrap(gateTrapId);

            if (gateTrapModel == null)
            {
                return;
            }

            var gateTrapController = new MatchEnvironmentGateTrapController(gateTrapModel, _gamePlayConfig.ExponentialDecay);
            gateTrapController.CreateWallView(_wallViewPrefab, _gateTrapsParent.transform);
            _gateTrapControllers.Add(gateTrapController);
        }

        public void UpdateGateTrapViews()
        {
            foreach (var gateTrapController in _gateTrapControllers)
            {
                gateTrapController.UpdateView();
            }
        }

        public void DestroyAll()
        {
            foreach (var gateTrapController in _gateTrapControllers)
            {
                gateTrapController.Destroy();
            }

            _gateTrapControllers.Clear();
        }
    }
}
