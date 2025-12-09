using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchNetEventsDataService
    {
        private ushort _bulletSpawnNetEventCounter = 0;
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed

        public MatchNetEventsDataService()
        {
            BulletSpawnNetEvents = new List<BulletSpawnNetEventS2C>();
        }

        public void AddBulletSpawnNetEvent(ushort bulletId, Vector2 position)
        {
            BulletSpawnNetEvents.Add(new BulletSpawnNetEventS2C(_bulletSpawnNetEventCounter++, bulletId, position));
        }
    }
}