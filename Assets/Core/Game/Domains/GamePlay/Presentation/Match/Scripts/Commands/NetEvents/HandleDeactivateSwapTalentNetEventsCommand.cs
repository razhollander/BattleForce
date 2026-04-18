using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateSwapTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ISwapFieldControllers _swapFieldControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _swapFieldControllers = _diContainer.Resolve<ISwapFieldControllers>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.DeactivateSwapTalentNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.DeactivateSwapTalentNetEvents)
            {
                _swapFieldControllers.DestroySwapField(netEvent.SwapFieldId);
            }

            _cachedPresentationEventsService.DeactivateSwapTalentNetEvents.Clear();
        }
    }
}
