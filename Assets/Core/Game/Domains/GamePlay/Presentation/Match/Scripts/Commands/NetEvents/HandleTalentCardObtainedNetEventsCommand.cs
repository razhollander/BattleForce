using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleTalentCardObtainedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ITalentCardControllers _talentCardControllers;
        private ITalentCardObtainedEffectController _talentCardObtainedEffectController;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _talentCardObtainedEffectController = _diContainer.Resolve<ITalentCardObtainedEffectController>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var talentCardObtainedNetEvents = _cachedPresentationEventsService.TalentCardObtainedNetEvents;
            if (talentCardObtainedNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var talentCardObtainedNetEvent in talentCardObtainedNetEvents)
            {
                var talentCardPosition = _talentCardControllers.GetTalentCardPosition(talentCardObtainedNetEvent.TalentCardId);
                var playerPosition = _playerControllers.GetPlayerPosition(talentCardObtainedNetEvent.ObtainedByPlayerId);
                _talentCardObtainedEffectController.PlayEffect(talentCardPosition, playerPosition);
                _talentCardControllers.DestroyTalentCard(talentCardObtainedNetEvent.TalentCardId);
            }
            
            talentCardObtainedNetEvents.Clear();
        }
    }
}