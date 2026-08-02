using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    /// <summary>
    /// Ends a Whac-A-Mole stage once its countdown expires. The team that whacked the most moles wins;
    /// every team tied for the lead is awarded the same amount of gems.
    /// </summary>
    public class TryEndWhacAMoleStageCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IStageDataService _stageDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private ICommandFactory _commandFactory;
        private StageEndedCommand _stageEndedCommand;

        private int _processedTick;

        public TryEndWhacAMoleStageCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _stageEndedCommand = _commandFactory.CreateCommandVoid<StageEndedCommand>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;
            var isCountdownOver = simulationState.StageType == StageType.WhacAMole
                                  && !simulationState.IsInPreparationPhase
                                  && !_stageDataService.IsStageEnded
                                  && _processedTick >= simulationState.WhacAMoleEndTick;

            if (!isCountdownOver)
            {
                return;
            }

            var highestMolesHit = GetHighestMolesHit();
            AwardGemsToLeadingTeams(highestMolesHit);
            _stageEndedCommand
                .SetWinningTeamId(GetLowestTeamIdWithMolesHit(highestMolesHit))
                .SetProcessedTick(_processedTick)
                .Execute();
        }

        private int GetHighestMolesHit()
        {
            var highestMolesHit = 0;

            foreach (var kvp in _matchDataService.SimulationState.MolesHitPerTeamId)
            {
                if (kvp.Value > highestMolesHit)
                {
                    highestMolesHit = kvp.Value;
                }
            }

            return highestMolesHit;
        }

        // The stage end event carries a single winner, so ties resolve to the lowest team id to stay deterministic.
        private ushort GetLowestTeamIdWithMolesHit(int molesHit)
        {
            var winningTeamId = ushort.MaxValue;

            foreach (var kvp in _matchDataService.SimulationState.MolesHitPerTeamId)
            {
                if (kvp.Value == molesHit && kvp.Key < winningTeamId)
                {
                    winningTeamId = kvp.Key;
                }
            }

            return winningTeamId;
        }

        private void AwardGemsToLeadingTeams(int highestMolesHit)
        {
            var simulationState = _matchDataService.SimulationState;
            var gemsForWinningTeam = _gamePlayConfigService.GamePlayConfig.WhacAMole.GemsForWinningTeam;

            foreach (var kvp in simulationState.MolesHitPerTeamId)
            {
                if (kvp.Value != highestMolesHit)
                {
                    continue;
                }

                simulationState.GemsPerTeamId[kvp.Key] += gemsForWinningTeam;
                _stageDataService.AddGemsForTeam(kvp.Key, gemsForWinningTeam);
            }
        }
    }
}
