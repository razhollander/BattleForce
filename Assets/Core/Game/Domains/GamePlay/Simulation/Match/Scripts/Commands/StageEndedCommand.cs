using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
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
            var playerToFocusOn = GetPlayerToFocusOn();
            _netEventsDataService.AddStageEndNetEvent(_processedTick, _winningTeamId, _stageDataService.GemsCollectedPerTeam, _matchDataService.SimulationState.GemsPerTeamId, playerToFocusOn.Id);
            _stageDataService.IsStageEnded = true;
            _stageDataService.StageRestartTimer = _gamePlayConfigService.GamePlayConfig.StageRestartDelaySeconds;
        }

        private PlayerStateS2C GetPlayerToFocusOn()
        {
            if (_matchDataService.SimulationState.StageType.IsBonusStage())
            {
                return GetTopScoringPlayerInWinningTeam();
            }

            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (player.Spaceship.IsAlive && player.TeamId == _winningTeamId)
                {
                    return player;
                }
            }
            
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (player.Spaceship.IsAlive && player.TeamId == _winningTeamId)
                {
                    return player;
                }
            }

            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (player.TeamId == _winningTeamId)
                {
                    return player;
                }
            }

            LogService.LogError("Somehow didnt find player to focus on");
            return null;
        }

        private PlayerStateS2C GetTopScoringPlayerInWinningTeam()
        {
            var stageScorePerPlayerId = _matchDataService.SimulationState.StageScorePerPlayerId;
            PlayerStateS2C topScoringPlayer = null;

            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (player.TeamId != _winningTeamId)
                {
                    continue;
                }

                if (topScoringPlayer == null)
                {
                    topScoringPlayer = player;
                    continue;
                }
                
                
                ushort scoreOfPlayer = stageScorePerPlayerId[player.Id];
                ushort scoreOfTopScoringPlayer = stageScorePerPlayerId[topScoringPlayer.Id];

                if (scoreOfPlayer > scoreOfTopScoringPlayer || (scoreOfPlayer == scoreOfTopScoringPlayer && player.Id < topScoringPlayer.Id)) // Deterministic tie-break: keep the lowest player id when scores are equal
                {
                    topScoringPlayer = player;
                }
            }

            if (topScoringPlayer == null)
            {
                LogService.LogError("Somehow didnt find player to focus on");
            }

            return topScoringPlayer;
        }
    }
}
