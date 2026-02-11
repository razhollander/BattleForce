using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloorTracker;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands
{
    public class HandleIfAnyPlayerChangedTeamFloorCommand: BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchMakingDataService;
        private IPlayersOnTeamFloorTrackerService _playersOnTeamFloorTrackerService;
        private IStartMatchWallController _startMatchWallController;
        private INetEventsDataService _netEventsDataService;
        private ICommandFactory _commandFactory;
        private HandleIfStartMatchEligiblityChangedCommand _handleIfStartMatchEligiblityChangedCommand;
        
        private int _tick;

        public HandleIfAnyPlayerChangedTeamFloorCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _playersOnTeamFloorTrackerService = _diContainer.Resolve<IPlayersOnTeamFloorTrackerService>();
            _startMatchWallController = _diContainer.Resolve<IStartMatchWallController>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _handleIfStartMatchEligiblityChangedCommand = _commandFactory.CreateCommandVoid<HandleIfStartMatchEligiblityChangedCommand>();
        }

        public void Execute()
        {
            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())
            {
                var playerId = playerState.Id;
                var newTeamId = _playersOnTeamFloorTrackerService.GetPlayerTeam(playerId);
                var didPlayerSwitchTeams = playerState.TeamId != newTeamId;
                
                if (didPlayerSwitchTeams)
                {
                    HandlePlayerChangedTeam(playerState, newTeamId);
                }
            }
        }

        private void HandlePlayerChangedTeam(MatchMakingPlayerStateS2C playerState, ushort newTeamId)
        {
            playerState.TeamId = newTeamId;
            _startMatchWallController.TryStopCountdown(_tick);
            _netEventsDataService.AddPlayerSwitchTeamNetEvent(_tick, playerState.Id, newTeamId);
            _handleIfStartMatchEligiblityChangedCommand.SetTick(_tick).Execute();
        }
    }
}