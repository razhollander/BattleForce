using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class BreakEggCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private INetEventsDataService _netEventsDataService;

        private ushort _eggId;
        private int _processedTick;

        public BreakEggCommand SetEggId(ushort eggId)
        {
            _eggId = eggId;
            return this;
        }

        public BreakEggCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            _netEventsDataService.AddChickenEggHitNetEventS2C(_processedTick, _eggId);
            _physicsSimulator.RemoveChickenEgg(_eggId);
            _matchDataService.SimulationState.RemoveChickenEggById(_eggId);
        }
    }
}
