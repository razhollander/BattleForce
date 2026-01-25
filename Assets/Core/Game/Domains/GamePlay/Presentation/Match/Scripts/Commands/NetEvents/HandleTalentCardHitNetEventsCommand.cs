using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleTalentCardHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ITalentCardControllers _talentCardControllers;

        public override void ResolveDependencies()
        {
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var talentCardHitNetEvents = _cachedPresentationEventsService.TalentCardHitNetEvents;
            if (talentCardHitNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            talentCardHitNetEvents.Sort();
            foreach (var talentCardHitNetEvent in talentCardHitNetEvents)
            {
                _talentCardControllers.DisplayTalentCardTakeDamaged(talentCardHitNetEvent.TalentCardId);
            }
            
            talentCardHitNetEvents.Clear();
        }
    }
}