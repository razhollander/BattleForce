using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public class MatchNetEventsDataService : IMatchNetEventsDataService
    {
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; set; } = new ();
    }
}