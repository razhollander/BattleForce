using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall
{
    public class TryShootLockedOnWallCommand : BaseCommand, ICommandVoid
    {
        private ILockOnWallTimerService _lockOnWallTimerService;
        private IStartMatchWallController _startMatchWallController;

        private int _tick;
        private ushort _casterPlayerId;

        public TryShootLockedOnWallCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public TryShootLockedOnWallCommand SetCasterPlayerId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
            return this;
        }

        public override void ResolveDependencies()
        {
            _lockOnWallTimerService = _diContainer.Resolve<ILockOnWallTimerService>();
            _startMatchWallController = _diContainer.Resolve<IStartMatchWallController>();
        }

        public void Execute()
        {
            if (!_lockOnWallTimerService.IsShootable(_casterPlayerId))
            {
                return;
            }

            _startMatchWallController.TryToggleCountdownState(_tick);
            _lockOnWallTimerService.ResetTimer(_casterPlayerId);
        }
    }
}
