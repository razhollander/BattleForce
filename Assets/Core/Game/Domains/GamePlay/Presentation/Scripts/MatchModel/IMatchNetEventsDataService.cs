using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public interface IMatchNetEventsDataService
    {
        List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; set; }
    }
}