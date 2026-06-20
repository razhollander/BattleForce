using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerLockedOnTargetHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private ILockOnTargetShootEffectController _lockOnTargetShootEffectController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _lockOnTargetShootEffectController = _diContainer.Resolve<ILockOnTargetShootEffectController>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PlayerLockedOnTargetHitNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.PlayerLockedOnTargetHitNetEvents)
            {
                var casterHeadPosition = _matchPlayerControllers.GetPlayerHeadTransform(netEvent.CasterPlayerId).position.ToVector2XY();
                var targetHeartPosition = _matchPlayerControllers.GetPlayerHeartTransform(netEvent.HitPlayerId).position.ToVector2XY();
                _lockOnTargetShootEffectController.Play(casterHeadPosition, targetHeartPosition);
            }

            _cachedPresentationEventsService.PlayerLockedOnTargetHitNetEvents.Clear();
        }
    }
}
