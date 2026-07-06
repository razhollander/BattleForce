using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePreparationPhaseEndedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IEnvironmentFieldBarrierControllers _environmentFieldBarrierControllers;
        private IPreparationPhaseCountdownController _preparationPhaseCountdownController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _environmentFieldBarrierControllers = _diContainer.Resolve<IEnvironmentFieldBarrierControllers>();
            _preparationPhaseCountdownController = _diContainer.Resolve<IPreparationPhaseCountdownController>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PreparationPhaseEndedNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var phaseEndedNetEvent in events)
            {
                _environmentFieldBarrierControllers.DestroyAll();
                _preparationPhaseCountdownController.StopCountdown();
            }

            events.Clear();
        }
    }
}
