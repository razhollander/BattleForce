using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryEndBonusStageByTimerCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IStageDataService _stageDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ICommandFactory _commandFactory;
        private StageEndedCommand _stageEndedCommand;

        private int _processedTick;

        public TryEndBonusStageByTimerCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _stageEndedCommand = _commandFactory.CreateCommandVoid<StageEndedCommand>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;
            var isCountdownOver = simulationState.StageType.IsBonusStage()
                                  && !simulationState.IsInPreparationPhase
                                  && !_stageDataService.IsStageEnded
                                  && _processedTick >= simulationState.WhacAMoleEndTick;

            if (!isCountdownOver)
            {
                return;
            }

            if (simulationState.StageType == StageType.WhacAMole)
            {
                HideAllMoles();
            }

            var highestScore = GetHighestScore();
            var winningTeamId = GetLowestTeamIdWithScore(highestScore);
            AwardGemsByRank();
            _stageEndedCommand
                .SetWinningTeamId(winningTeamId)
                .SetProcessedTick(_processedTick)
                .Execute();
        }

        // No per-mole hide net event is sent: the client hides every mole on its own once it sees the stage end to save network bandwidth,
        // so here the moles only need to leave the physics simulation and the state.
        private void HideAllMoles()
        {
            var moles = _matchDataService.SimulationState.Moles;

            for (var i = moles.Count - 1; i >= 0; i--)
            {
                ref var mole = ref moles.GetByIndex(i);

                if (mole.IsEmerged)
                {
                    _physicsSimulator.RemoveMole(mole.Id);
                }
            }

            moles.Clear();
        }

        private int GetHighestScore()
        {
            var highestScore = 0;

            foreach (var kvp in _matchDataService.SimulationState.StageScorePerTeamId)
            {
                if (kvp.Value > highestScore)
                {
                    highestScore = kvp.Value;
                }
            }

            return highestScore;
        }

        // The stage end event carries a single winner, so ties resolve to the lowest team id to stay deterministic.
        private ushort GetLowestTeamIdWithScore(int score)
        {
            var winningTeamId = ushort.MaxValue;

            foreach (var kvp in _matchDataService.SimulationState.StageScorePerTeamId)
            {
                if (kvp.Value == score && kvp.Key < winningTeamId)
                {
                    winningTeamId = kvp.Key;
                }
            }

            return winningTeamId;
        }

        // Each team earns one gem for every team it strictly outscored. With N teams the total pool is
        // 0+1+...+(N-1); teams tied on score each count zero teams between them, so they receive the
        // same lower-placed amount and the higher place they share is skipped.
        private void AwardGemsByRank()
        {
            var simulationState = _matchDataService.SimulationState;
            var stageScorePerTeamId = simulationState.StageScorePerTeamId;

            foreach (var team in stageScorePerTeamId)
            {
                var teamsStrictlyBelow = 0;

                foreach (var otherTeam in stageScorePerTeamId)
                {
                    if (otherTeam.Value < team.Value)
                    {
                        teamsStrictlyBelow++;
                    }
                }

                if (teamsStrictlyBelow <= 0)
                {
                    continue;
                }

                simulationState.GemsPerTeamId[team.Key] += teamsStrictlyBelow;
                _stageDataService.AddGemsForTeam(team.Key, teamsStrictlyBelow);
            }
        }
    }
}
