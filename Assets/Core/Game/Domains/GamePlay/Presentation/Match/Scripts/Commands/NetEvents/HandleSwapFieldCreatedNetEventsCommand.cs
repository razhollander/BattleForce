using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleSwapFieldCreatedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;
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
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
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
                var swapFieldModel = _matchDataService.GetSwapField(swapFieldCreatedEvent.SwapFieldId);
                var currentRadius = swapFieldModel.CalculateCurrentRadiusForTick(_tick);
                _swapFieldControllers.CreateSwapField(swapFieldCreatedEvent.SwapFieldId, currentRadius, playerPosition);
            }

            _cachedPresentationEventsService.CreateSwapFieldNetEvents.Clear();
        }
    }
}
