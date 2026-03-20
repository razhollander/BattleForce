using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
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
        private IMatchPlayerUIControllers _matchPlayerUIControllers;
        private IMatchDataService _matchDataService;
        private int _currentServerTick;

        public HandleTalentCardObtainedNetEventsCommand SetCurrentServerTick(int currentServerTick)
        {
            _currentServerTick = currentServerTick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _talentCardObtainedEffectController = _diContainer.Resolve<ITalentCardObtainedEffectController>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
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
                var obtainedByPlayerId = talentCardObtainedNetEvent.ObtainedByPlayerId;
                var playerPosition = _playerControllers.GetPlayerPosition(obtainedByPlayerId);
                _talentCardObtainedEffectController.PlayEffect(talentCardPosition, playerPosition);
                _talentCardControllers.DestroyTalentCard(talentCardObtainedNetEvent.TalentCardId);
                _matchPlayerUIControllers.UpdatePlayerTalents(obtainedByPlayerId, talentCardObtainedNetEvent.PlayerTalents, _currentServerTick);
                
                var isFirstTalentObtained = talentCardObtainedNetEvent.PlayerTalents.Count == 1;
                if (isFirstTalentObtained || talentCardObtainedNetEvent.DidReplaceTalent)
                {
                    _playerControllers.SetPlayerTalentSelected(obtainedByPlayerId, _matchDataService.GetPlayer(obtainedByPlayerId).Spaceship.TalentsState.SelectedTalentIndex);
                }
            }
            
            talentCardObtainedNetEvents.Clear();
        }
    }
}