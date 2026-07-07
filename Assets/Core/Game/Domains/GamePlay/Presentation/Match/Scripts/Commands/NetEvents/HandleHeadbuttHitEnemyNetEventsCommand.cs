using Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleHeadbuttHitEnemyNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IHeadbuttHitEffectController _headbuttHitEffectController;
        private IMatchPlayerControllers _playerControllers;
        private IAudioService _audioService;
        private IWorldCameraController _worldCameraController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _headbuttHitEffectController = _diContainer.Resolve<IHeadbuttHitEffectController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.HeadbuttHitEnemyNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var hitEvent in events)
            {
                var casterPosition = _playerControllers.GetPlayerPosition(hitEvent.CasterPlayerId);
                var enemyPosition = _playerControllers.GetPlayerPosition(hitEvent.EnemyPlayerId);
                var hitPosition = (casterPosition + enemyPosition) * 0.5f;
                _headbuttHitEffectController.PlayEffect(hitPosition);
                _playerControllers.HidePlayerHeadbuttHelmet(hitEvent.CasterPlayerId);
            }
            
            _audioService.PlayAudio(AudioClipType.HeadbuttHit);
            _worldCameraController.ShakeCamera(10,0.25f);
            _cachedPresentationEventsService.HeadbuttHitEnemyNetEvents.Clear();
        }
    }
}
