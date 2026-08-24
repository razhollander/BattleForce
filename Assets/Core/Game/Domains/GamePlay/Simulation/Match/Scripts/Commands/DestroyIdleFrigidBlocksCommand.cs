using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class DestroyIdleFrigidBlocksCommand : BaseCommand, ICommandVoid
    {
        private IFrigidBlocksController _frigidBlocksController;

        private int _tick;
        private float _deltaTime;

        public DestroyIdleFrigidBlocksCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public DestroyIdleFrigidBlocksCommand SetDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _frigidBlocksController = _diContainer.Resolve<IFrigidBlocksController>();
        }

        public void Execute()
        {
            _frigidBlocksController.DestroyIdleBlocks(_tick, _deltaTime);
        }
    }
}
