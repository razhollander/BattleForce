using Core.Game.Domains.GamePlay.Presentation.Match.Features.GainBoltEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.AudioService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleGainBoltsNetEventCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IGainBoltEffectController _gainBoltEffectController;
        private IMatchDataService _matchDataService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _gainBoltEffectController = _diContainer.Resolve<IGainBoltEffectController>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var gainBoltsNetEvents = _cachedPresentationEventsService.GainBoltsNetEvents;
            if (gainBoltsNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var gainBoltsEvent in gainBoltsNetEvents)
            {
                var player = _matchDataService.GetPlayer(gainBoltsEvent.PlayerId);
                _teamsBoardUIController.UpdateTeamBolts(player.TeamId, gainBoltsEvent.TotalTeamBolts);

                var playerTransform = _matchPlayerControllers.GetPlayerTransform(gainBoltsEvent.PlayerId);
                var effectSpawnPosition = playerTransform.position.ToVector2XY() + player.Spaceship.Transform.Radius * Vector2.up;
                _gainBoltEffectController.PlayEffect(gainBoltsEvent.GainedAmount, effectSpawnPosition, playerTransform);
                _audioService.PlayAudio(AudioClipType.GainBolts, AudioChannelType.Fx);
            }

            gainBoltsNetEvents.Clear();
        }
    }
}
