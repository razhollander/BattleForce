using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchNetEventsDataService : IMatchNetEventsDataService
    {
        public Dictionary<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; private set; } // todo: remove events related to bullet when bullet is destroyed
        public Dictionary<ushort, FixedUnorderedList<PlayerJoinAcceptPacketS2C>> JoinAcceptNetEventsPerPlayer { get; private set; } // todo: remove events related to player when player is destroyed
        public Dictionary<ushort, FixedUnorderedList<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        public Dictionary<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        private readonly ConcurrentPool<FixedUnorderedList<BulletSpawnNetEventS2C>> _bulletSpawnListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerJoinAcceptPacketS2C>> _joinAcceptListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerTakeDamageNetEventS2C>> _playerTakeDamageListPool;
        private readonly ConcurrentPool<FixedUnorderedList<BulletDestroyedNetEventS2C>> _bulletDestroyedListPool;

        public MatchNetEventsDataService(NetworkConfig networkConfig)
        {
            var maxConcurrentPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            BulletSpawnNetEventsPerPlayer = new Dictionary<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>>(maxConcurrentPlayers);
            JoinAcceptNetEventsPerPlayer = new Dictionary<ushort, FixedUnorderedList<PlayerJoinAcceptPacketS2C>>(maxConcurrentPlayers);
            PlayerTakeDamageNetEventsPerPlayer = new Dictionary<ushort, FixedUnorderedList<PlayerTakeDamageNetEventS2C>>(maxConcurrentPlayers);
            BulletDestroyedNetEventsPerPlayer = new Dictionary<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>>(maxConcurrentPlayers);
            _bulletSpawnListPool = new ConcurrentPool<FixedUnorderedList<BulletSpawnNetEventS2C>>(() => new FixedUnorderedList<BulletSpawnNetEventS2C>(networkConfig.MaxCap.BulletSpawnNetEvents), maxConcurrentPlayers);
            _joinAcceptListPool = new ConcurrentPool<FixedUnorderedList<PlayerJoinAcceptPacketS2C>>(() => new FixedUnorderedList<PlayerJoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents), maxConcurrentPlayers);
            _playerTakeDamageListPool = new ConcurrentPool<FixedUnorderedList<PlayerTakeDamageNetEventS2C>>(() => new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(networkConfig.MaxCap.PlayerTakeDamageNetEvents), maxConcurrentPlayers);
            _bulletDestroyedListPool = new ConcurrentPool<FixedUnorderedList<BulletDestroyedNetEventS2C>>(() => new FixedUnorderedList<BulletDestroyedNetEventS2C>(networkConfig.MaxCap.BulletDestroyedNetEvents), maxConcurrentPlayers);
        }
        
        public void StartSavingPlayerEvents(ushort playerId)
        {
            if (!BulletSpawnNetEventsPerPlayer.TryAdd(playerId, _bulletSpawnListPool.Get()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!JoinAcceptNetEventsPerPlayer.TryAdd(playerId, _joinAcceptListPool.Get()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!PlayerTakeDamageNetEventsPerPlayer.TryAdd(playerId, _playerTakeDamageListPool.Get()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!BulletDestroyedNetEventsPerPlayer.TryAdd(playerId, _bulletDestroyedListPool.Get()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
        }
        
        public void StopSavingPlayerEvents(ushort playerId)
        {
            var bulletSpawnedList = BulletSpawnNetEventsPerPlayer[playerId];
            bulletSpawnedList.Clear();
            _bulletSpawnListPool.Return(bulletSpawnedList);
            var joinAcceptedList = JoinAcceptNetEventsPerPlayer[playerId];
            joinAcceptedList.Clear();
            _joinAcceptListPool.Return(joinAcceptedList);
            var playerTakeDamageedList = PlayerTakeDamageNetEventsPerPlayer[playerId];
            playerTakeDamageedList.Clear();
            _playerTakeDamageListPool.Return(playerTakeDamageedList);
            var bulletDestroyededList = BulletDestroyedNetEventsPerPlayer[playerId];
            bulletDestroyededList.Clear();
            _bulletDestroyedListPool.Return(bulletDestroyededList);
            
            BulletSpawnNetEventsPerPlayer.Remove(playerId);
            JoinAcceptNetEventsPerPlayer.Remove(playerId);
            PlayerTakeDamageNetEventsPerPlayer.Remove(playerId);
            BulletDestroyedNetEventsPerPlayer.Remove(playerId);
        }
        
        public void AddPlayerTakeDamageNetEvent(int onTick, ushort damagedPlayerId, ushort playerHealth, ushort hitDamage, bool isAlive)
        {
            foreach (var kvp in PlayerTakeDamageNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = damagedPlayerId;
                packet.PlayerHealth = playerHealth;
                packet.HitDamage = hitDamage;
                packet.IsAlive = isAlive;
            }
        }

        public void AddBulletDestroyedNetEvent(int onTick, ushort bulletId, Vector2 position)
        {
            foreach (var kvp in BulletDestroyedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.BulletId = bulletId;
                packet.Position = position;
            }
        }

        public void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius)
        {
            foreach (var kvp in BulletSpawnNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.BulletId = bulletId;
                packet.BelongToPlayerId = belongToPlayerId;
                packet.Position = position;
                packet.BulletRadius = bulletRadius;
            }
        }

        public void AddPlayerJoinAcceptedEvent(int onTick, PlayerStateS2C playerState, SimulationStateS2C simulationState)
        {
            foreach (var kvp in JoinAcceptNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.IsLocal = playerState.Id == kvp.Key;
                packet.PlayerState = playerState;
                packet.SimulationState = simulationState;
            }
        }

        public void RemoveAllEventsOlderThanTick(ushort playerId, int tick)
        {
            if (BulletSpawnNetEventsPerPlayer.TryGetValue(playerId, out var bulletSpawnNetEvents))
            {
                for (int i = bulletSpawnNetEvents.Count - 1; i >= 0; i--)
                {
                    if(bulletSpawnNetEvents[i].OccuredOnTick<tick)
                    {
                        bulletSpawnNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (JoinAcceptNetEventsPerPlayer.TryGetValue(playerId, out var joinAcceptNetEvents))
            {
                for (int i = joinAcceptNetEvents.Count - 1; i >= 0; i--)
                {
                    if(joinAcceptNetEvents[i].OccuredOnTick < tick)
                    {
                        joinAcceptNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerTakeDamageNetEventsPerPlayer.TryGetValue(playerId, out var playerTakeDamageNetEvents))
            {
                for (int i = playerTakeDamageNetEvents.Count - 1; i >= 0; i--)
                {
                    if(playerTakeDamageNetEvents[i].OccuredOnTick < tick)
                    {
                        playerTakeDamageNetEvents.RemoveAt(i);
                    }
                }
            }

            if (BulletDestroyedNetEventsPerPlayer.TryGetValue(playerId, out var bulletDestroyedNetEvents))
            {
                for (int i = bulletDestroyedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (bulletDestroyedNetEvents[i].OccuredOnTick < tick)
                    {
                        bulletDestroyedNetEvents.RemoveAt(i);
                    }
                }
            }
        }
    }
}