using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs
{
    public class MatchMakingEnvironmentWallsControllers : IMatchMakingEnvironmentWallsControllers
    {
        private readonly IMatchMakingDataService _matchDataService;
        private readonly EnvironmentWallView _wallViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly List<MatchMakingEnvironmentWallController> _wallControllers = new ();
        private readonly Dictionary<int, List<MatchMakingEnvironmentWallController>> _wallsByTeamId = new();
        private GameObject _wallsParent;

        public MatchMakingEnvironmentWallsControllers(IMatchMakingDataService matchDataService, EnvironmentWallView wallViewPrefab, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchDataService = matchDataService;
            _wallViewPrefab = wallViewPrefab;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _wallsParent = new GameObject("EnvironmentWallsParent");

            // 1. Create wrap-around walls (boundaries)
            var boundaryWalls = _sharedGamePlayConfig.MatchMakingEnvironment.GetWalls();
            foreach (var wall in boundaryWalls)
            {
                _matchDataService.AddWall(wall);
                CreateWall(wall.Id);
            }

            // 2. Create team floor walls
            var teamFloors = DonutQuadrantWalls.GenerateQuadrantWallPerTeam(
                _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsRadius,
                _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsPrecision);

            foreach (var kvp in teamFloors)
            {
                var teamId = (int)kvp.Key;
                var walls = kvp.Value;

                // Get team color
                Color teamColor = Color.white;
                if (_sharedGamePlayConfig.ColorPerTeamId.TryGetValue(teamId, out var color))
                {
                    teamColor = color;
                }

                if (!_wallsByTeamId.ContainsKey(teamId))
                {
                    _wallsByTeamId[teamId] = new List<MatchMakingEnvironmentWallController>();
                }

                foreach (var wall in walls)
                {
                    _matchDataService.AddWall(wall);
                    var controller = CreateWall(wall.Id);
                    controller.SetColor(teamColor);
                    _wallsByTeamId[teamId].Add(controller);
                }
            }
        }

        public MatchMakingEnvironmentWallController CreateWall(ushort wallId)
        {
            var wallController = new MatchMakingEnvironmentWallController(wallId, _matchDataService);
            wallController.CreateWallView(_wallViewPrefab, _wallsParent.transform);
            _wallControllers.Add(wallController);
            return wallController;
        }

        public void AnimateWall(int teamId)
        {
            if (_wallsByTeamId.TryGetValue(teamId, out var controllers))
            {
                foreach (var controller in controllers)
                {
                    controller.AnimateBounce();
                }
            }
        }
    }
}