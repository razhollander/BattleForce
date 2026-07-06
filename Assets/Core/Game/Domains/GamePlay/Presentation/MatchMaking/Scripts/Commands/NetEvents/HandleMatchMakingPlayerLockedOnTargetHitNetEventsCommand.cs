using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandleMatchMakingPlayerLockedOnTargetHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private static readonly Vector2 WALL_CENTER = Vector2.zero;

        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchMakingDataService _matchMakingDataService;
        private ILockOnTargetShootEffectController _lockOnTargetShootEffectController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
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
                var headPosition = _matchMakingDataService.GetPlayer(netEvent.CasterPlayerId).Spaceship.Transform.GetHeadPosition().ToUnityVector2();
                _lockOnTargetShootEffectController.Play(headPosition, WALL_CENTER);
            }

            _cachedPresentationEventsService.PlayerLockedOnTargetHitNetEvents.Clear();
        }
    }
}
