using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class SimulationNetEventsHandler
    {
        private readonly IMatchDataService _matchDataService;
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;

        public SimulationNetEventsHandler(IMatchDataService matchDataService, IMatchNetEventsDataService matchNetEventsDataService)
        {
            _matchDataService = matchDataService;
            _matchNetEventsDataService = matchNetEventsDataService;
        }

        public void ProcessBulletSpawnEvents(StructPool<PlayerBulletS2C> bullets, List<BulletSpawnNetEventS2C> bulletSpawnNetEvents)
        {
            if (bulletSpawnNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            _matchNetEventsDataService.BulletSpawnNetEvents.AddRange(bulletSpawnNetEvents);
            
            foreach (var bulletSpawnNetEvent in bulletSpawnNetEvents)
            {
                var spawnedBulletId = bulletSpawnNetEvent.BulletId;
                foreach (var index in bullets.UsedIndices())
                {
                    var currentBullet = bullets[index];
                    if (currentBullet.Id == spawnedBulletId)
                    {
                        var bullet = currentBullet;
                        _matchDataService.AddBullet(bulletSpawnNetEvent.BulletId, bullet.BelongToPlayerId,
                            bulletSpawnNetEvent.Position);
                    }
                }
            }
        }
    }
}