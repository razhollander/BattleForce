using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
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