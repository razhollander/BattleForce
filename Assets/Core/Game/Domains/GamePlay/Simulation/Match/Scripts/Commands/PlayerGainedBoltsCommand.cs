using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class PlayerGainedBoltsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private ushort _playerId;
        private int _gainedAmount;
        private int _processedTick;

        public PlayerGainedBoltsCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public PlayerGainedBoltsCommand SetGainedAmount(int gainedAmount)
        {
            _gainedAmount = gainedAmount;
            return this;
        }

        public PlayerGainedBoltsCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var player = _matchDataService.SimulationState.GetPlayerById(_playerId);
            var teamId = player.TeamId;

            if (!_matchDataService.SimulationState.BoltsPerTeam.ContainsKey(teamId))
            {
                 _matchDataService.SimulationState.BoltsPerTeam.Add(teamId, 0);
            }

            _matchDataService.SimulationState.BoltsPerTeam[teamId] += _gainedAmount;
            var totalBolts = _matchDataService.SimulationState.BoltsPerTeam[teamId];

            _netEventsDataService.AddGainBoltsNetEvent(_processedTick, _playerId, _gainedAmount, totalBolts);
        }
    }
}
