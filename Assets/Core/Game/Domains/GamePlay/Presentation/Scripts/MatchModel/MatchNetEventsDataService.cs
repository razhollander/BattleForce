using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public class MatchNetEventsDataService : IMatchNetEventsDataService
    {
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; set; } = new ();
        public List<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents { get; set; } = new();
        public List<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents { get; set; } = new();
    }
}