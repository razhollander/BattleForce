using Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateYearsOfPainTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IYearsOfPainEffectController _yearsOfPainEffectController;
        private IMatchPlayerControllers _matchPlayerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _yearsOfPainEffectController = _diContainer.Resolve<IYearsOfPainEffectController>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents)
            {
                var casterTransform = _matchPlayerControllers.GetPlayerSpaceshipTransform(netEvent.CasterPlayerId);
                _yearsOfPainEffectController.PlayFieldEffect(casterTransform, netEvent.Direction.ToUnityVector2());

                if (netEvent.HasHit)
                {
                    var enemyPosition = _matchPlayerControllers.GetPlayerPosition(netEvent.HitEnemyId);
                    _yearsOfPainEffectController.PlayHitEffect(enemyPosition);
                }
            }

            _cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents.Clear();
        }
    }
}
