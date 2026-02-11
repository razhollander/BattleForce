using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchEligibilityLogic
{
    public interface IStartMatchEligibilityLogicService
    {
        bool IsEligibleToStartMatch();
    }

    public class StartMatchEligibilityLogicService : IStartMatchEligibilityLogicService
    {
        private readonly IMatchMakingDataService _matchMakingDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly HashSet<ushort> _cachedDifferentTeamIdsAssignedToPlayers;
        
        public StartMatchEligibilityLogicService(IMatchMakingDataService matchMakingDataService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchMakingDataService = matchMakingDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _cachedDifferentTeamIdsAssignedToPlayers = new HashSet<ushort>(sharedGamePlayConfig.MaxTeamsAmount);
        }

        public bool IsEligibleToStartMatch()
        {
            _cachedDifferentTeamIdsAssignedToPlayers.Clear();
            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())
            {
                var isPlayerInNoTeam = playerState.TeamId == _sharedGamePlayConfig.NoTeamId;

                if (isPlayerInNoTeam)
                {
                    return false;
                }

                _cachedDifferentTeamIdsAssignedToPlayers.Add(playerState.TeamId);
            }

            return _cachedDifferentTeamIdsAssignedToPlayers.Count > 1 || _matchMakingDataService.SimulationState.Players.Count == 1;
        }
    }
}