using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.GateTraps.Scripts.Mvc
{
    public class MatchEnvironmentGateTrapsControllers : IMatchEnvironmentGateTrapsControllers
    {
        private const string PARENT_GAME_OBJECT_NAME = "EnvironmentGateTrapsParent";

        private readonly GateTrapView _wallViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchEnvironmentGateTrapController> _gateTrapControllers = new();
        private GameObject _gateTrapsParent;

        public MatchEnvironmentGateTrapsControllers(GateTrapView wallViewPrefab, PresentationGamePlayConfig gamePlayConfig)
        {
            _wallViewPrefab = wallViewPrefab;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _gateTrapsParent = new GameObject(PARENT_GAME_OBJECT_NAME);
        }

        public void CreateGateTrap(MatchEnvironmentGateTrapModel gateTrapModel)
        {
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
