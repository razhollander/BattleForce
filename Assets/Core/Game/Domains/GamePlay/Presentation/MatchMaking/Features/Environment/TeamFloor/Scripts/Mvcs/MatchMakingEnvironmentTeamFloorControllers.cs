using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs
{
    public class MatchMakingEnvironmentTeamFloorControllers : IMatchMakingEnvironmentTeamFloorControllers
    {
        private readonly EnvironmentTeamFloorsView _teamFloorsViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly Dictionary<ushort, MatchMakingEnvironmentTeamFloorController> _controllerByTeamId = new();
        private GameObject _teamFloorsParent;

        public MatchMakingEnvironmentTeamFloorControllers(EnvironmentTeamFloorsView teamFloorsViewPrefab, SharedGamePlayConfig sharedGamePlayConfig, PresentationGamePlayConfig gamePlayConfig)
        {
            _teamFloorsViewPrefab = teamFloorsViewPrefab;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _teamFloorsParent = new GameObject("EnvironmentTeamFloorsParent");
            var teamFloors = DonutQuadrantWalls.GenerateQuadrantWallsPerTeam(
                _sharedGamePlayConfig.TeamIds,
                _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsRadius,
                _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsPrecision,
                _sharedGamePlayConfig.MinEntityId);

            foreach (var kvp in teamFloors)
            {
                var teamId = kvp.Key;
                var walls = kvp.Value;
                var teamFloorController = new MatchMakingEnvironmentTeamFloorController(teamId, walls, _gamePlayConfig);
                teamFloorController.CreateTeamFloors(_teamFloorsViewPrefab, _teamFloorsParent.transform);
                _controllerByTeamId[teamId] = teamFloorController;
            }
        }

        public void AnimateFloorBounce(ushort teamId)
        {
            _controllerByTeamId[teamId].AnimateBounce();
        }
    }
}