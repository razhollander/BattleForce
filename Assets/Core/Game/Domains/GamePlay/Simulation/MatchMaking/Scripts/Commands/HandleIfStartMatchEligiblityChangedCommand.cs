using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchEligibilityLogic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands
{
    public class HandleIfStartMatchEligiblityChangedCommand : BaseCommand, ICommandVoid
    {
        private IStartMatchEligibilityLogicService _startMatchEligibilityLogicService;
        private IMatchMakingDataService _matchMakingDataService;
        private INetEventsDataService _netEventsDataService;
        
        private int _processedTick;

        public HandleIfStartMatchEligiblityChangedCommand SetTick(int tick)
        {
            _processedTick = tick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _startMatchEligibilityLogicService = _diContainer.Resolve<IStartMatchEligibilityLogicService>();
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var isCurrentlyEligibleToStartMatch = _matchMakingDataService.SimulationState.StartMatchWall.IsEnabled;
            var newIsEligibleToStartMatch = _startMatchEligibilityLogicService.IsEligibleToStartMatch();
            var didEligibilityChange = newIsEligibleToStartMatch != isCurrentlyEligibleToStartMatch;

            if (didEligibilityChange)
            {
                _matchMakingDataService.SimulationState.StartMatchWall.IsEnabled = newIsEligibleToStartMatch;
                _netEventsDataService.AddStartMatchEligibleChangedNetEvent(_processedTick, newIsEligibleToStartMatch);
            }
        }
    }
}