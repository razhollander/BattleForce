using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchNetEventsDataService : IMatchNetEventsDataService
    {
        public Dictionary<ushort, List<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; private set; } = new (); // todo: remove events related to bullet when bullet is destroyed
        public Dictionary<ushort, List<PlayerJoinAcceptPacketS2C>> JoinAcceptNetEventsPerPlayer { get; private set; } = new (); // todo: remove events related to player when player is destroyed
        
        public Dictionary<ushort, List<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerPlayer { get; private set; } = new(); // todo: remove events related to player hit when player is destroyed
        public Dictionary<ushort, List<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerPlayer { get; private set; } = new(); // todo: remove events related to player hit when player is destroyed

        public void StartSavingPlayerEvents(ushort playerId)
        {
            if (!BulletSpawnNetEventsPerPlayer.TryAdd(playerId, new List<BulletSpawnNetEventS2C>()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!JoinAcceptNetEventsPerPlayer.TryAdd(playerId, new List<PlayerJoinAcceptPacketS2C>()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!PlayerTakeDamageNetEventsPerPlayer.TryAdd(playerId, new List<PlayerTakeDamageNetEventS2C>()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!BulletDestroyedNetEventsPerPlayer.TryAdd(playerId, new List<BulletDestroyedNetEventS2C>()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
        }
        
        public void StopSavingPlayerEvents(ushort playerId)
        {
            BulletSpawnNetEventsPerPlayer.Remove(playerId);
            JoinAcceptNetEventsPerPlayer.Remove(playerId);
            PlayerTakeDamageNetEventsPerPlayer.Remove(playerId);
            BulletDestroyedNetEventsPerPlayer.Remove(playerId);
        }
        
        public void AddPlayerTakeDamageNetEvent(int onTick, ushort damagedPlayerId, int playerHealth, int hitDamage, bool isAlive)
        {
            foreach (var kvp in PlayerTakeDamageNetEventsPerPlayer)
            {
                kvp.Value.Add(new PlayerTakeDamageNetEventS2C
                {
                    OccuredOnTick = onTick,
                    PlayerId = damagedPlayerId,
                    PlayerHealth = playerHealth,
                    HitDamage = hitDamage,
                    IsAlive = isAlive
                });
            }
        }

        public void AddBulletDestroyedNetEvent(int onTick, ushort bulletId, Vector2 position)
        {
            foreach (var kvp in BulletDestroyedNetEventsPerPlayer)
            {
                kvp.Value.Add(new BulletDestroyedNetEventS2C(onTick, bulletId, position));
            }
        }

        public void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius)
        {
            foreach (var kvp in BulletSpawnNetEventsPerPlayer)
            {
                kvp.Value.Add(new BulletSpawnNetEventS2C(onTick, bulletId, belongToPlayerId, position, bulletRadius));
            }
        }

        public void AddPlayerJoinAcceptedEvent(int onTick, PlayerStateS2C playerState, SimulationStateS2C simulationState)
        {
            foreach (var kvp in JoinAcceptNetEventsPerPlayer)
            {
                kvp.Value.Add(new PlayerJoinAcceptPacketS2C
                {
                    OccuredOnTick = onTick,
                    IsLocal = playerState.Id == kvp.Key,
                    PlayerState = playerState,
                    SimulationState = simulationState
                });
            }
        }

        public void RemoveAllEventsOlderThanTick(ushort playerId, int tick)
        {
            if (BulletSpawnNetEventsPerPlayer.TryGetValue(playerId, out var bulletSpawnNetEvents))
            {
                bulletSpawnNetEvents.RemoveAll(x => x.OccuredOnTick < tick);
            }
            if (JoinAcceptNetEventsPerPlayer.TryGetValue(playerId, out var joinAcceptNetEvents))
            {
                joinAcceptNetEvents.RemoveAll(x => x.OccuredOnTick < tick);
            }
            if (PlayerTakeDamageNetEventsPerPlayer.TryGetValue(playerId, out var playerTakeDamageNetEvents))
            {
                playerTakeDamageNetEvents.RemoveAll(x => x.OccuredOnTick < tick);
            }
            if (BulletDestroyedNetEventsPerPlayer.TryGetValue(playerId, out var bulletDestroyedNetEvents))
            {
                bulletDestroyedNetEvents.RemoveAll(x => x.OccuredOnTick < tick);
            }
        }
    }
}