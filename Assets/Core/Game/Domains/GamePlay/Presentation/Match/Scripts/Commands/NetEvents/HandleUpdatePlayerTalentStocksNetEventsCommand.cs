using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleUpdatePlayerTalentStocksNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchDataService _matchDataService;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents)
            {
                var casterPlayerId = netEvent.CasterPlayerId;

                if (_matchDataService.GetPlayer(casterPlayerId).Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalentForCaster))
                {
                    _playerControllers.UpdateIsPlayerArrowShownAccordingToTalentState(casterPlayerId, currentSelectedTalentForCaster);
                }
            }
         
            _cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Clear();
        }
    }
}
