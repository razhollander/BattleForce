using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandleMatchMakingPlayerLockOnTargetsChangedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchMakingPlayerControllers _playerControllers;
        private ILockOnTargetEffectController _lockOnTargetEffectController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PlayerLockOnTargetsChangedNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.PlayerLockOnTargetsChangedNetEvents)
            {
                var playerId = netEvent.PlayerId;
                var targets = netEvent.LockedOnTargetObjects;
                var isLockingOnWall = targets.Count > 0;
                _playerControllers.SetIsLockOnTargetSightShownForPlayer(playerId, isLockingOnWall);
                _lockOnTargetEffectController.RefreshTargetEffectsOfCaster(playerId, targets);
            }

            _cachedPresentationEventsService.PlayerLockOnTargetsChangedNetEvents.Clear();
        }
    }
}
