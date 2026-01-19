using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class HandleBulletSpawnNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;
        private IBulletControllers _bulletControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IBulletControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
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
                var bulletColor = _matchDataService.GetPlayer(bulletsSpawnEvent.BelongToPlayerId).Spaceship.Color;
                _bulletControllers.CreateBullet(bulletId, bulletsSpawnEvent.BulletRadius, bulletsSpawnEvent.Position, bulletColor);
                _playerControllers.ShootBulletEffectForPlayer(bulletsSpawnEvent.BelongToPlayerId);
            }
            
            bulletsSpawnEvents.Clear();
        }
    }
}