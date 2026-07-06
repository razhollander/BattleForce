using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall
{
    public class TryShootLockedOnWallCommand : BaseCommand, ICommandVoid
    {
        private ILockOnWallTimerService _lockOnWallTimerService;
        private IStartMatchWallController _startMatchWallController;
        private IMatchMakingDataService _matchMakingDataService;
        private INetEventsDataService _netEventsDataService;
        private SharedGamePlayConfig _sharedGamePlayConfig;

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
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _startMatchWallController = _diContainer.Resolve<IStartMatchWallController>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        {
            if (!_matchMakingDataService.SimulationState.GetPlayerById(_casterPlayerId).Spaceship.IsLockingOnWallShootable)
            {
                return;
            }

            _startMatchWallController.TryToggleCountdownState(_tick);
            _lockOnWallTimerService.ResetPlayerTimer(_casterPlayerId);
            _netEventsDataService.AddPlayerLockedOnTargetHitNetEvent(_tick, _casterPlayerId, _sharedGamePlayConfig.MinEntityId);
        }
    }
}
