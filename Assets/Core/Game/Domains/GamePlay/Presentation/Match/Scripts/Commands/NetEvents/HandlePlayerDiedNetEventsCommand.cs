using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerDiedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerUIControllers _matchPlayerUIControllers;
        private IMatchDataService _matchDataService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _matchPlayerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var playerDiedEvents = _cachedPresentationEventsService.PlayerDiedNetEvents;
            if (playerDiedEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerDiedEvent in playerDiedEvents)
            {
                var playerId = playerDiedEvent.PlayerId;
                 
                _matchPlayerControllers.HidePlayerHealthBar(playerDiedEvent.PlayerId);
                _matchPlayerControllers.SetIsTailWaving(playerDiedEvent.PlayerId, false);
                _matchPlayerControllers.SetIsDeadAuraEnabled(playerDiedEvent.PlayerId, true);
                _matchPlayerUIControllers.SwitchToPlayerDeadState(playerId);
                _audioService.PlayAudio(AudioClipType.PlayerDied);
            }

            playerDiedEvents.Clear();
        }
    }
}
