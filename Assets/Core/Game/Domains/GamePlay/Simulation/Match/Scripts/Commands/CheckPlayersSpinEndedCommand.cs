using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class CheckPlayersSpinEndedCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;

        private Dictionary<ushort, bool> _wasSpinningDict = new Dictionary<ushort, bool>();
        private int _tick;

        public CheckPlayersSpinEndedCommand SetTick(int tick)
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
                var isSpinningNow = player.Spaceship.Transform.AngularVelocity != 0;
                _wasSpinningDict.TryGetValue(player.Id, out var wasSpinning);

                if (wasSpinning && !isSpinningNow)
                {
                    _netEventsDataService.AddPlayerSpinnedEndedNetEvent(_tick, player.Id);
                }

                _wasSpinningDict[player.Id] = isSpinningNow;
            }
        }
    }
}
