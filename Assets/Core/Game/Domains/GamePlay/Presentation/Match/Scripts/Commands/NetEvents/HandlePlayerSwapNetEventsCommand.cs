using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class HandlePlayerSwapNetEventsCommand: BaseCommand, ICommandVoid
    {
        private IPlayerControllers _playerControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
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