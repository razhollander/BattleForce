using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleSwapFieldCreatedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private ISwapFieldControllers _swapFieldControllers;

        private int _tick;

        public HandleSwapFieldCreatedNetEventsCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _swapFieldControllers = _diContainer.Resolve<ISwapFieldControllers>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.CreateSwapFieldNetEvents.Count == 0)
            {
                return;
            }

            foreach (var swapFieldCreatedEvent in _cachedPresentationEventsService.CreateSwapFieldNetEvents)
            {
                var playerPosition = _matchPlayerControllers.GetPlayerPosition(swapFieldCreatedEvent.CasterPlayerId);
                var currentRadius = MathUtils.Remap(swapFieldCreatedEvent.OccuredOnTick,
                    swapFieldCreatedEvent.EndOnTick, 0, swapFieldCreatedEvent.MaxRadius, _tick); // we intentioanly don't use the model in the match data service, because we may not have it in an edge case that the field was created and destroyed at the same tick.
                _swapFieldControllers.CreateSwapField(swapFieldCreatedEvent.SwapFieldId, currentRadius, playerPosition);
            }

            _cachedPresentationEventsService.CreateSwapFieldNetEvents.Clear();
        }
    }
}
