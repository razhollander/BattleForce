using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public interface IMatchNetEventsDataService
    {
        List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; set; }
        List<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents { get; set; }
        List<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents { get; set; }
    }
}