using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryBreakChickenEggsOnEmergedMoleCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private TryHitMoleCommand _tryHitMoleCommand;
        private BreakEggCommand _breakEggCommand;

        private ushort _moleId;
        private int _processedTick;

        public TryBreakChickenEggsOnEmergedMoleCommand SetMoleId(ushort moleId)
        {
            _moleId = moleId;
            return this;
        }

        public TryBreakChickenEggsOnEmergedMoleCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _tryHitMoleCommand = _diContainer.Resolve<ICommandFactory>().CreateCommandVoid<TryHitMoleCommand>();
            _breakEggCommand = _diContainer.Resolve<ICommandFactory>().CreateCommandVoid<BreakEggCommand>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;

            if (simulationState.ChickenEggs.Count == 0)
            {
                return;
            }

            var molePosition = simulationState.GetMoleById(_moleId).Position;
            var moleRadius = _gamePlayConfigService.GamePlayConfig.WhacAMole.MoleRadius;

            for (var eggIndex = simulationState.ChickenEggs.Count - 1; eggIndex >= 0; eggIndex--) // backwards, since a broken egg is removed from the list
            {
                var egg = simulationState.ChickenEggs[eggIndex];
                var casterPlayerState = simulationState.GetPlayerById(egg.PlayerCasterId);
                var breakDistance = casterPlayerState.Spaceship.Transform.Radius + moleRadius;

                if ((molePosition - egg.Position).LengthSquared() > breakDistance * breakDistance)
                {
                    continue;
                }

                _tryHitMoleCommand
                    .SetMoleId(_moleId)
                    .SetByPlayerId(egg.PlayerCasterId)
                    .SetByTeamId(casterPlayerState.TeamId)
                    .SetProcessedTick(_processedTick)
                    .Execute();

                _breakEggCommand.SetEggId(egg.Id).SetProcessedTick(_processedTick).Execute();

                if (!simulationState.TryGetMoleIndexById(_moleId, out _)) // the mole was whacked, the remaining eggs have nothing left to break on
                {
                    return;
                }
            }
        }
    }
}
