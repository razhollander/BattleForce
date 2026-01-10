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
        public CapacityDict<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; private set; } // todo: remove events related to bullet when bullet is destroyed
        public CapacityDict<ushort, FixedClassUnorderedList<PlayerJoinAcceptPacketS2C>> JoinAcceptNetEventsPerPlayer { get; private set; } // todo: remove events related to player when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<PlayersSwapNetEventS2C>> PlayerSwapNetEventsPerPlayer { get; private set;} // todo: remove events related to player hit when player is destroyed
        private readonly ConcurrentPool<FixedUnorderedList<BulletSpawnNetEventS2C>> _bulletSpawnListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<PlayerJoinAcceptPacketS2C>> _joinAcceptListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerTakeDamageNetEventS2C>> _playerTakeDamageListPool;
        private readonly ConcurrentPool<FixedUnorderedList<BulletDestroyedNetEventS2C>> _bulletDestroyedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayersSwapNetEventS2C>> _playerSwapListPool;

        public MatchNetEventsDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            var maxConcurrentPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            BulletSpawnNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>>(maxConcurrentPlayers);
            JoinAcceptNetEventsPerPlayer = new CapacityDict<ushort, FixedClassUnorderedList<PlayerJoinAcceptPacketS2C>>(maxConcurrentPlayers);
            PlayerTakeDamageNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayerTakeDamageNetEventS2C>>(maxConcurrentPlayers);
            BulletDestroyedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>>(maxConcurrentPlayers);
            PlayerSwapNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayersSwapNetEventS2C>>(maxConcurrentPlayers);
            
            _bulletSpawnListPool = new ConcurrentPool<FixedUnorderedList<BulletSpawnNetEventS2C>>(() => new FixedUnorderedList<BulletSpawnNetEventS2C>(networkConfig.MaxCap.BulletSpawnNetEvents), maxConcurrentPlayers);
            _joinAcceptListPool = new ConcurrentPool<FixedClassUnorderedList<PlayerJoinAcceptPacketS2C>>(() =>
            {
                var list =new FixedClassUnorderedList<PlayerJoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents, ()=>new PlayerJoinAcceptPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer));
                list.Clear();
                return list;
            }, maxConcurrentPlayers);
            _playerTakeDamageListPool = new ConcurrentPool<FixedUnorderedList<PlayerTakeDamageNetEventS2C>>(() => new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(networkConfig.MaxCap.PlayerTakeDamageNetEvents), maxConcurrentPlayers);
            _bulletDestroyedListPool = new ConcurrentPool<FixedUnorderedList<BulletDestroyedNetEventS2C>>(() => new FixedUnorderedList<BulletDestroyedNetEventS2C>(networkConfig.MaxCap.BulletDestroyedNetEvents), maxConcurrentPlayers);
            _playerSwapListPool= new ConcurrentPool<FixedUnorderedList<PlayersSwapNetEventS2C>>(() => new FixedUnorderedList<PlayersSwapNetEventS2C>(networkConfig.MaxCap.PlayerSwapNetEvents), maxConcurrentPlayers);
        }
        
        public void StartSavingPlayerEvents(ushort playerId)
        {
            if (!BulletSpawnNetEventsPerPlayer.ContainsKey(playerId)) // don't use TryAdd since it will _bulletSpawnListPool.Get() an object from the pool! 
            {
                BulletSpawnNetEventsPerPlayer.Add(playerId, _bulletSpawnListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!JoinAcceptNetEventsPerPlayer.ContainsKey(playerId))
            {
                JoinAcceptNetEventsPerPlayer.Add(playerId, _joinAcceptListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!PlayerTakeDamageNetEventsPerPlayer.ContainsKey(playerId))
            {
                PlayerTakeDamageNetEventsPerPlayer.Add(playerId, _playerTakeDamageListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!BulletDestroyedNetEventsPerPlayer.ContainsKey(playerId))
            {
                BulletDestroyedNetEventsPerPlayer.Add(playerId, _bulletDestroyedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }    
            
            if (!PlayerSwapNetEventsPerPlayer.ContainsKey(playerId))
            {
                PlayerSwapNetEventsPerPlayer.Add(playerId, _playerSwapListPool.Get());
            }
            else
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
            var playerSwapList = PlayerSwapNetEventsPerPlayer[playerId];
            playerSwapList.Clear();
            _playerSwapListPool.Return(playerSwapList);
            
            BulletSpawnNetEventsPerPlayer.Remove(playerId);
            JoinAcceptNetEventsPerPlayer.Remove(playerId);
            PlayerTakeDamageNetEventsPerPlayer.Remove(playerId);
            BulletDestroyedNetEventsPerPlayer.Remove(playerId);
            PlayerSwapNetEventsPerPlayer.Remove(playerId);
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
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.IsLocal = playerState.Id == kvp.Key;
                packet.PlayerState = playerState;
                packet.SimulationState = simulationState;
            }
        }

        public void AddPlayersSwapEvent(int onTick, ushort casterPlayerId, ushort otherPlayerId, Vector2 casterPlayerPosition, Vector2 otherPlayerPosition, Vector2 casterPlayerDirection, Vector2 otherPlayerDirection)
        {
            foreach (var kvp in PlayerSwapNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.OtherPlayerId = otherPlayerId;
                packet.CasterPosition = casterPlayerPosition;
                packet.OtherPosition = otherPlayerPosition;
                packet.CasterDirection = casterPlayerDirection;
                packet.OtherDirection = otherPlayerDirection;
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
            
            if (PlayerSwapNetEventsPerPlayer.TryGetValue(playerId, out var playerSwapNetEvents))
            {
                for (int i = playerSwapNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerSwapNetEvents[i].OccuredOnTick < tick)
                    {
                        playerSwapNetEvents.RemoveAt(i);
                    }
                }
            }
        }
    }
}