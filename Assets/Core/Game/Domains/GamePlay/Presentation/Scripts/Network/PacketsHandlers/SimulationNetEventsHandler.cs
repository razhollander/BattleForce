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

        public void ProcessBulletSpawnEvents(List<BulletSpawnNetEventS2C> bulletSpawnNetEvents)
        {
            if (bulletSpawnNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var bulletSpawnNetEvent in bulletSpawnNetEvents)
            {
                if (bulletSpawnNetEvent.SequenceId > _matchNetEventsDataService.HighestBulletSpawnEventSequenceId)
                {
                    _matchDataService.AddBullet(bulletSpawnNetEvent.BulletId, bulletSpawnNetEvent.BelongToPlayerId,
                        bulletSpawnNetEvent.Position);
                    _matchNetEventsDataService.BulletSpawnNetEvents.Add(bulletSpawnNetEvent);
                    _matchNetEventsDataService.HighestBulletSpawnEventSequenceId = bulletSpawnNetEvent.SequenceId;
                }
            }
        }
    }
}