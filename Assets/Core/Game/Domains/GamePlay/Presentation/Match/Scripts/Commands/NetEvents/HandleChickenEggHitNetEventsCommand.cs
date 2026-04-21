using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleChickenEggHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchChickenEggsControllers _chickenEggsControllers;
        private IStageCancellationTokenProvider _stageCancellationTokenProvider;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _stageCancellationTokenProvider = _diContainer.Resolve<IStageCancellationTokenProvider>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.ChickenEggHitNetEvents;
            if (events.IsNullOrEmpty()) return;

            foreach (var evt in events)
            {
                _chickenEggsControllers.BreakAndDestroyEgg(evt.EggId, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
            }

            events.Clear();
        }
    }
}
