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

        public StartMatchEligibilityLogicService(IMatchMakingDataService matchMakingDataService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchMakingDataService = matchMakingDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public bool IsEligibleToStartMatch()
        {
            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())
            {
                if (playerState.TeamId == _sharedGamePlayConfig.NoTeamId)
                {
                    return false;
                }
            }

            return true;
        }
    }
}