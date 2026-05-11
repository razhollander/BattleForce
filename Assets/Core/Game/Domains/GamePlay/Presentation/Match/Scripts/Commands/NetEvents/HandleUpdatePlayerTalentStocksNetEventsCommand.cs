using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleUpdatePlayerTalentStocksNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Count == 0)
            {
                return;
            }
            
            _cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Clear();
        }
    }
}
