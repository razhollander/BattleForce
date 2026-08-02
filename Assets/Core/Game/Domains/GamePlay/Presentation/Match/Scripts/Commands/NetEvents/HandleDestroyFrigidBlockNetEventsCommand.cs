using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDestroyFrigidBlockNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IFrigidBlocksControllers _frigidBlocksControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _frigidBlocksControllers = _diContainer.Resolve<IFrigidBlocksControllers>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.DestroyFrigidBlockNetEvents;
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                _frigidBlocksControllers.DestroyFrigidBlock(netEvent.BlockId);
            }

            _cachedPresentationEventsService.DestroyFrigidBlockNetEvents.Clear();
        }
    }
}
