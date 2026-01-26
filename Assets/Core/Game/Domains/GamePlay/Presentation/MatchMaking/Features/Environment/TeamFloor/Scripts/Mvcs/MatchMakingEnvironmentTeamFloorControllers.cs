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

        public MatchMakingEnvironmentTeamFloorControllers(EnvironmentTeamFloorsView teamFloorsViewPrefab, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _teamFloorsViewPrefab = teamFloorsViewPrefab;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _teamFloorsParent = new GameObject("EnvironmentTeamFloorsParent");
            var teamFloors = DonutQuadrantWalls.GenerateQuadrantWallPerTeam(
                _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsRadius,
                _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsPrecision);

            foreach (var kvp in teamFloors)
            {
                var teamId = kvp.Key;
                var walls = kvp.Value;
                var teamFloorController = new MatchMakingEnvironmentTeamFloorController(teamId, walls, _sharedGamePlayConfig);
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