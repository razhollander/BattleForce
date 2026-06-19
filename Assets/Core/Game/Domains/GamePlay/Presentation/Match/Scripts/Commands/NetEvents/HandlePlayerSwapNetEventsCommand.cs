using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerSwapNetEventsCommand: BaseCommand, ICommandVoid
    {
        private IMatchPlayerControllers _playerControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var playersSwapEvents = _cachedPresentationEventsService.PlayerSwapNetEvents;
            if (playersSwapEvents.IsNullOrEmpty())
            {
                return;
            }
            
            _audioService.PlayAudio(AudioClipType.Swap); // play only once no matter how many events received
            //BlinkPlayersWithoutSwapEffect(playersSwapEvents);
            playersSwapEvents.Clear();
        }

        // private void BlinkPlayersWithoutSwapEffect(List<PlayersSwapNetEventS2C> playersSwapEvents)
        // {
        //     foreach (var playersSwapEvent in playersSwapEvents)
        //     {
        //         _playerControllers.SetPlayerTransform(playersSwapEvent.CasterPlayerId, playersSwapEvent.CasterPosition, playersSwapEvent.CasterDirection);
        //         _playerControllers.SetPlayerTransform(playersSwapEvent.OtherPlayerId, playersSwapEvent.OtherPosition, playersSwapEvent.OtherDirection);
        //     }
        // }
    }
}