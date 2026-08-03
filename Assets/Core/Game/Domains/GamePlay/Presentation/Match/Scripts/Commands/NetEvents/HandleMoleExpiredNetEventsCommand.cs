using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleMoleExpiredNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMoleControllers _moleControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
        }

        public void Execute()
        {
            var moleExpiredNetEvents = _cachedPresentationEventsService.MoleExpiredNetEvents;

            if (moleExpiredNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleExpiredNetEvent in moleExpiredNetEvents)
            {
                _moleControllers.SetMoleInHole(moleExpiredNetEvent.MoleId);
            }

            moleExpiredNetEvents.Clear();
        }
    }
}
