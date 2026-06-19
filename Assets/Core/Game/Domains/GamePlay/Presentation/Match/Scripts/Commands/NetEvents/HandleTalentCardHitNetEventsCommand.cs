using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleTalentCardHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ITalentCardControllers _talentCardControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
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
                _audioService.PlayAudio(AudioClipType.TalentCardHit);
            }
            
            talentCardHitNetEvents.Clear();
        }
    }
}