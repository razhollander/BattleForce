using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public class MatchNetEventsDataService : IMatchNetEventsDataService
    {
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; set; } = new List<BulletSpawnNetEventS2C>();
        public int HighestBulletSpawnEventSequenceId { get; set; }
    }
}