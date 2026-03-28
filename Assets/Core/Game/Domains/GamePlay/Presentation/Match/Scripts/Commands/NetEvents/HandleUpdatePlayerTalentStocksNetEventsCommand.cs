using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleUpdatePlayerTalentStocksNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerUIControllers _playerUIControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Count == 0)
            {
                return;
            }

            // for (int i = 0; i < _cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Count; i++)
            // {
            //     var eventData = _cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents[i];
            //     _playerUIControllers.UpdatePlayerTalentStocks(eventData.CasterPlayerId, eventData.TalentType, eventData.CurrentStocksAmount, eventData.RecieveNextStockOnTick);
            // }
            _cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Clear();
        }
    }
}
