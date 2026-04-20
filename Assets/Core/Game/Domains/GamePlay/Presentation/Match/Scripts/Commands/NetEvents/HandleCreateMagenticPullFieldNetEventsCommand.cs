using Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleCreateMagenticPullFieldNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMagneticPullEffectController _magneticPullEffectController;
        private IMatchPlayerControllers _matchPlayerControllers;
        private SharedGamePlayConfig _sharedConfig;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _magneticPullEffectController = _diContainer.Resolve<IMagneticPullEffectController>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _sharedConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.CreateMagenticPullFieldNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.CreateMagenticPullFieldNetEvents)
            {
                _magneticPullEffectController.PlayFieldEffect(netEvent.Position.ToUnityVector2(), netEvent.Direction.ToUnityVector2(), _sharedConfig.MagneticPullFieldRadius, null);

                if (netEvent.HasHit)
                {
                    var enemyPosition = _matchPlayerControllers.GetPlayerPosition(netEvent.HitEnemyId);
                    var casterPosition = _matchPlayerControllers.GetPlayerPosition(netEvent.CasterPlayerId);
                    _magneticPullEffectController.PlayHitEffect(casterPosition, enemyPosition, null);
                }
            }

            _cachedPresentationEventsService.CreateMagenticPullFieldNetEvents.Clear();
        }
    }
}