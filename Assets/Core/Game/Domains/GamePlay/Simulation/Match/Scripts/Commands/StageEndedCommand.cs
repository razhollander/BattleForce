using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StageEndedCommand : BaseCommand, ICommandVoid
    {
        private ushort _winningTeamId;
        private int _processedTick;

        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private IStageDataService _stageDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        
        private ushort _playerIdDoingWinningBlow;

        public StageEndedCommand PlayerIdDoingWinningBlow(ushort playerId)
        {
            _playerIdDoingWinningBlow = playerId;
            return this;
        }
        
        public StageEndedCommand SetWinningTeamId(ushort winningTeamId)
        {
            _winningTeamId = winningTeamId;
            return this;
        }

        public StageEndedCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
        }

        public void Execute()
        {
            LogService.LogTopic($"Match Ended! Winning Team: {_winningTeamId}", LogTopicType.ServerNetwork);
            _matchDataService.SimulationState.CurrentStageWinnerTeamId = _winningTeamId;
            _matchDataService.SimulationState.IsInShowoffWinners = true;
            _netEventsDataService.AddStageEndNetEvent(_processedTick, _winningTeamId, _stageDataService.GemsCollectedPerTeam, _matchDataService.SimulationState.GemsPerTeamId, _playerIdDoingWinningBlow);
            _stageDataService.IsStageEnded = true;
            _stageDataService.StageRestartTimer = _gamePlayConfigService.GamePlayConfig.StageRestartDelaySeconds;
        }
    }
}
