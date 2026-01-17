using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions.Linq;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using Sirenix.Utilities;
using UnityEngine;

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
            
            //var areAnyBulletsDestroyed = !_matchNetEventsDataService.BulletDestroyedNetEvents.IsNullOrEmpty();
            foreach (var bulletsSpawnEvent in bulletsSpawnEvents)
            {
                var bulletId = bulletsSpawnEvent.BulletId;
                //var isBulletDestroyed = areAnyBulletsDestroyed && _matchNetEventsDataService.BulletDestroyedNetEvents.Any(x => x.BulletId == bulletId);
                //if (!isBulletDestroyed)
                //{
                var bulletColor = _matchDataService.GetPlayer(bulletsSpawnEvent.BelongToPlayerId).Spaceship.Color;
                _bulletControllers.CreateBullet(bulletId, bulletsSpawnEvent.BelongToPlayerId, bulletsSpawnEvent.BulletRadius, bulletsSpawnEvent.Position, bulletColor);
                //}
                
                _playerControllers.ShootBulletEffectForPlayer(bulletsSpawnEvent.BelongToPlayerId);
            }
            
            bulletsSpawnEvents.Clear();
        }
    }
}