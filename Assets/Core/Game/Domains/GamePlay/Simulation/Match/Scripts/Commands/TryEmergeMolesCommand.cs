using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    /// <summary>
    /// Brings out every mole whose hole finished shaking. A mole gets its physics body only here, so until then
    /// nothing can target or hit it while it is still hidden.
    /// </summary>
    public class TryEmergeMolesCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private TryBreakChickenEggsOnEmergedMoleCommand _tryBreakChickenEggsOnEmergedMoleCommand;

        private int _processedTick;

        public TryEmergeMolesCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _tryBreakChickenEggsOnEmergedMoleCommand = _diContainer.Resolve<ICommandFactory>().CreateCommandVoid<TryBreakChickenEggsOnEmergedMoleCommand>();
        }

        public void Execute()
        {
            if (_matchDataService.SimulationState.StageType != StageType.WhacAMole)
            {
                return;
            }

            var moles = _matchDataService.SimulationState.Moles;
            var moleRadius = _gamePlayConfigService.GamePlayConfig.WhacAMole.MoleRadius;

            for (var i = moles.Count - 1; i >= 0; i--) // backwards, since a mole whacked by the eggs it emerged under is removed from the list
            {
                ref var mole = ref moles.GetByIndex(i);

                if (mole.IsEmerged || _processedTick < mole.EmergeOnTick)
                {
                    continue;
                }

                mole.IsEmerged = true;
                _physicsSimulator.AddMole(mole.Id, mole.Position, moleRadius);
                _tryBreakChickenEggsOnEmergedMoleCommand.SetMoleId(mole.Id).SetProcessedTick(_processedTick).Execute();
            }
        }
    }
}
