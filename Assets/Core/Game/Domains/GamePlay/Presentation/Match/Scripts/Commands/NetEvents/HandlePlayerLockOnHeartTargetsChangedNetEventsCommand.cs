using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;

    public class HandlePlayerLockOnHeartTargetsChangedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private ILockOnTargetEffectController _lockOnTargetEffectController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PlayerLockOnHeartTargetsChangedNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.PlayerLockOnHeartTargetsChangedNetEvents)
            {
                var isLockOnHeartSightShown = netEvent.PlayerIdsLockedOnTarget.Count > 0;
                _matchPlayerControllers.SetPlayerIsLockOnHeartSightShown(netEvent.PlayerId, isLockOnHeartSightShown);
                _lockOnTargetEffectController.RefreshTargetEffectsOfCaster(netEvent.PlayerId, netEvent.PlayerIdsLockedOnTarget);
                netEvent.PlayerIdsLockedOnTarget.Clear();
            }


            _cachedPresentationEventsService.PlayerLockOnHeartTargetsChangedNetEvents.Clear();
        }
    }
}