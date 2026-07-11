using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryEndPlayersSpinIfReachedZeroAngularVecityCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        
        private int _tick;

        public TryEndPlayersSpinIfReachedZeroAngularVecityCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                var isPlayerSpinned = player.Spaceship.IsSpinned;
                var isPlayerAngularVelocityZero = player.Spaceship.Transform.AngularVelocity == 0;
                var isNoLongerSpinning = isPlayerAngularVelocityZero && isPlayerSpinned;
                if (!isNoLongerSpinning)
                {
                    continue;
                }

                player.Spaceship.IsSpinned = false;
                _netEventsDataService.AddPlayerSpinnedEndedNetEvent(_tick, player.Id);
            }
        }
    }
}
