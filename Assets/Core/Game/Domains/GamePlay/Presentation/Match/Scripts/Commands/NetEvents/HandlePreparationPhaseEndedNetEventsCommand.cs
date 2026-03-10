using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePreparationPhaseEndedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IEnvironmentFieldBarrierControllers _environmentFieldBarrierControllers;
        private DataService.IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _environmentFieldBarrierControllers = _diContainer.Resolve<IEnvironmentFieldBarrierControllers>();
            _matchDataService = _diContainer.Resolve<DataService.IMatchDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PreparationPhaseEndedNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            var lastEvent = events[events.Count - 1];
            _matchDataService.StartPhaseInitialTick = lastEvent.OccuredOnTick;

            _environmentFieldBarrierControllers.DestroyAll();
            events.Clear();
        }
    }
}
