using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerLockOnTargetsChangedNetEventsCommand : BaseCommand, ICommandVoid
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
            if (_cachedPresentationEventsService.PlayerLockOnTargetsChangedNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.PlayerLockOnTargetsChangedNetEvents)
            {
                var isLockOnTargetSightShown = netEvent.LockedOnTargetObjects.HasNonRetainedTarget();
                _matchPlayerControllers.SetPlayerIsLockOnTargetSightShown(netEvent.PlayerId, isLockOnTargetSightShown);
                _lockOnTargetEffectController.RefreshTargetEffectsOfCaster(netEvent.PlayerId, netEvent.LockedOnTargetObjects);
            }

            _cachedPresentationEventsService.PlayerLockOnTargetsChangedNetEvents.Clear();
        }
    }
}