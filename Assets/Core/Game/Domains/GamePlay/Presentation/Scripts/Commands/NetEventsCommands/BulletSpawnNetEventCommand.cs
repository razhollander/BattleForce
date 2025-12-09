using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using Sirenix.Utilities;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class BulletSpawnNetEventCommand : BaseCommand, ICommandVoid
    {
        private IPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;
        private IBulletControllers _bulletControllers;
        private IMatchNetEventsDataService _matchNetEventsDataService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IBulletControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _matchNetEventsDataService = _diContainer.Resolve<IMatchNetEventsDataService>();
        }

        public void Execute()
        {
            var bulletsSpawnEvents = _matchNetEventsDataService.BulletSpawnNetEvents;
            if (bulletsSpawnEvents.IsNullOrEmpty())
            {
                return;
            }
            bulletsSpawnEvents.Sort();
            foreach (var bulletsSpawnEvent in bulletsSpawnEvents)
            {
                if (bulletsSpawnEvent.SequenceId <= _matchNetEventsDataService.HighestBulletSpawnEventSequenceId)
                {
                    LogService.LogTopic($"Bullet of seqId already processed {bulletsSpawnEvent.SequenceId}");
                    continue;
                }
                
                var bulletState = _matchDataService.GetBullet(bulletsSpawnEvent.BulletId);
                _bulletControllers.CreateBullet(bulletState.Id);
                _playerControllers.ShootBulletEffectForPlayer(bulletState.BelongToPlayerId);
                _matchNetEventsDataService.HighestBulletSpawnEventSequenceId = bulletsSpawnEvent.SequenceId;
            }
            
            _matchNetEventsDataService.BulletSpawnNetEvents.Clear();
        }
    }
}