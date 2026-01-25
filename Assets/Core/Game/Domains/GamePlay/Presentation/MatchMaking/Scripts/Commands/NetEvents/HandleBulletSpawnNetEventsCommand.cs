using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandleBulletSpawnNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingPlayerControllers _playerControllers;
        private IMatchMakingDataService _matchDataService;
        private IMatchMakingBulletControllers _bulletControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IMatchMakingBulletControllers>();
            _matchDataService = _diContainer.Resolve<IMatchMakingDataService>();
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