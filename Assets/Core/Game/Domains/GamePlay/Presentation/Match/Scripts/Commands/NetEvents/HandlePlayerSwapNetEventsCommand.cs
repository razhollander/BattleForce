using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerSwapNetEventsCommand: BaseCommand, ICommandVoid
    {
        private IMatchPlayerControllers _playerControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var playersSwapEvents = _cachedPresentationEventsService.PlayerSwapNetEvents;
            if (playersSwapEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var playersSwapEvent in playersSwapEvents)
            {
                _playerControllers.SetPlayerTransform(playersSwapEvent.CasterPlayerId, playersSwapEvent.CasterPosition, playersSwapEvent.CasterDirection);
            }
            
            playersSwapEvents.Clear();
        }
    }
}