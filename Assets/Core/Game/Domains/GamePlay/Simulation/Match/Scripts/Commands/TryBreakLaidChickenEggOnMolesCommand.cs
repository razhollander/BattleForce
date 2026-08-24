using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    /// <summary>
    /// Breaks a freshly laid chicken egg if it was laid on top of an already emerged mole, at the single moment the egg is laid.
    /// Both bodies are static and never move afterwards, so this overlap can only start here or when a mole emerges,
    /// which is why no per tick scan is needed.
    /// </summary>
    public class TryBreakLaidChickenEggOnMolesCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private TryHitMoleCommand _tryHitMoleCommand;
        private BreakEggCommand _breakEggCommand;

        private ushort _eggId;
        private int _processedTick;

        public TryBreakLaidChickenEggOnMolesCommand SetEggId(ushort eggId)
        {
            _eggId = eggId;
            return this;
        }

        public TryBreakLaidChickenEggOnMolesCommand SetProcessedTick(int processedTick)
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

            if (simulationState.Moles.Count == 0)
            {
                return;
            }

            var egg = simulationState.GetChickenEggById(_eggId);
            var casterPlayerState = simulationState.GetPlayerById(egg.PlayerCasterId);
            var moleRadius = _gamePlayConfigService.GamePlayConfig.WhacAMole.MoleRadius;
            var breakDistance = casterPlayerState.Spaceship.Transform.Radius + moleRadius;

            for (var moleIndex = 0; moleIndex < simulationState.Moles.Count; moleIndex++)
            {
                var mole = simulationState.Moles[moleIndex];

                if (!mole.IsEmerged || (mole.Position - egg.Position).LengthSquared() > breakDistance * breakDistance)
                {
                    continue;
                }

                _tryHitMoleCommand
                    .SetMoleId(mole.Id)
                    .SetByPlayerId(egg.PlayerCasterId)
                    .SetByTeamId(casterPlayerState.TeamId)
                    .SetProcessedTick(_processedTick)
                    .Execute();

                _breakEggCommand.SetEggId(egg.Id).SetProcessedTick(_processedTick).Execute();
                return;
            }
        }
    }
}
