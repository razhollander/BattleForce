using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchNetEventsDataService
    {
        public Dictionary<ushort, List<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer; // todo: remove events related to bullet when bullet id destroyed

        public MatchNetEventsDataService()
        {
            BulletSpawnNetEventsPerPlayer = new Dictionary<ushort, List<BulletSpawnNetEventS2C>>();
        }

        public void StartSavingPlayerEvents(ushort playerId)
        {
            if (!BulletSpawnNetEventsPerPlayer.TryAdd(playerId, new List<BulletSpawnNetEventS2C>()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
        }
        
        public void StopSavingPlayerEvents(ushort playerId)
        {
            BulletSpawnNetEventsPerPlayer.Remove(playerId);
        }
        
        public void AddBulletSpawnNetEvent(ushort onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position)
        {
            foreach (var kvp in BulletSpawnNetEventsPerPlayer)
            {
                kvp.Value.Add(new BulletSpawnNetEventS2C(onTick, bulletId, belongToPlayerId, position));
            }
        }

        public void RemoveAllEventsOlderThanTick(ushort playerId, int tick)
        {
            if (BulletSpawnNetEventsPerPlayer.TryGetValue(playerId, out var bulletSpawnNetEvents))
            {
                bulletSpawnNetEvents.RemoveAll(x => x.OccuredOnTick < tick);
            }
        }
    }
}