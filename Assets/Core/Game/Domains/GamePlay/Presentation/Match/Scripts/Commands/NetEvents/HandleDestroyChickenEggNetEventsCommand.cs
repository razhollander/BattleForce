using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDestroyChickenEggNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchChickenEggsControllers _chickenEggsControllers;
        private Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService.IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _matchDataService = _diContainer.Resolve<Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService.IMatchDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DestroyChickenEggNetEvents;
            if (events.IsNullOrEmpty()) return;

            foreach (var evt in events)
            {
                _matchDataService.RemoveChickenEgg(evt.EggId);
                _chickenEggsControllers.DestroyEgg(evt.EggId);
            }

            events.Clear();
        }
    }
}
