using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleBulletSpawnNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;
        private IMatchBulletControllers _bulletControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private PresentationGamePlayConfig _gameplayConfig;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IMatchBulletControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _gameplayConfig =_diContainer.Resolve<PresentationGamePlayConfig>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var bulletsSpawnEvents = _cachedPresentationEventsService.BulletSpawnNetEvents;
            if (bulletsSpawnEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var bulletsSpawnEvent in bulletsSpawnEvents)
            {
                var bulletId = bulletsSpawnEvent.BulletId;
                var bulletColor = _gameplayConfig.ColorPerTeamId[_matchDataService.GetPlayer(bulletsSpawnEvent.BelongToPlayerId).TeamId];
                _bulletControllers.CreateBullet(bulletId, bulletsSpawnEvent.BulletRadius, bulletsSpawnEvent.Position, bulletColor);
                _playerControllers.ShootBulletEffectForPlayer(bulletsSpawnEvent.BelongToPlayerId);
                _audioService.PlayAudio(AudioClipType.PlayerShoot);
            }
            
            bulletsSpawnEvents.Clear();
        }
    }
}