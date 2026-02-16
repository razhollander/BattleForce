using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NetEventsDataService : INetEventsDataService
    {
        public CapacityDict<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; private set; } // todo: remove events related to bullet when bullet is destroyed
        public CapacityDict<ushort, FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>> PlayerRejoinAcceptNetEventsPerPlayer { get; private set; } // todo: remove events related to player when player is destroyed
        public CapacityDict<ushort, FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>> MatchMakingPlayerJoinAcceptNetEventsPerPlayer { get; private set; } // todo: remove events related to player when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<PlayerDiedNetEventS2C>> PlayerDiedNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<PlayersSwapNetEventS2C>> PlayerSwapNetEventsPerPlayer { get; private set;} // todo: remove events related to player hit when player is destroyed
        public CapacityDict<ushort, FixedClassUnorderedList<TalentCardObtainedNetEventS2C>> TalentCardObtainedNetEventsPerPlayer { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<ushort, FixedUnorderedList<TalentCardHitNetEventS2C>> TalentCardHitNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> PowerUpBallSpawnedNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> PowerUpBallObtainedNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> PlayerSwitchTeamNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<StartMatchCountdownNetEventS2C>> StartMatchCountdownNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<StopMatchCountdownNetEventS2C>> StopMatchCountdownNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>> StartMatchEligibleChangedNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedClassUnorderedList<StageEndNetEventS2C>> StageEndNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<TeamLostNetEventS2C>> TeamLostNetEventsPerPlayer { get; }

        private readonly ConcurrentPool<FixedUnorderedList<BulletSpawnNetEventS2C>> _bulletSpawnListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>> _playerRejoinAcceptListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>> _matchMakingPlayerJoinAcceptListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerTakeDamageNetEventS2C>> _playerTakeDamageListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerDiedNetEventS2C>> _playerDiedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<BulletDestroyedNetEventS2C>> _bulletDestroyedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayersSwapNetEventS2C>> _playerSwapListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<TalentCardObtainedNetEventS2C>> _talentCardObtainedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<TalentCardHitNetEventS2C>> _talentCardHitListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> _powerUpBallsSpawnedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> _powerUpBallsObtainedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> _playerSwitchTeamListPool;
        private readonly ConcurrentPool<FixedUnorderedList<StartMatchCountdownNetEventS2C>> _startMatchCountdownListPool;
        private readonly ConcurrentPool<FixedUnorderedList<StopMatchCountdownNetEventS2C>> _stopMatchCountdownListPool;
        private readonly ConcurrentPool<FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>> _startMatchEligibleChangedListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<StageEndNetEventS2C>> _stageEndNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<TeamLostNetEventS2C>> _teamLostNetEventsListPool;

        public NetEventsDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            var maxConcurrentPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            BulletSpawnNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<BulletSpawnNetEventS2C>>(maxConcurrentPlayers);
            PlayerRejoinAcceptNetEventsPerPlayer = new CapacityDict<ushort, FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>>(maxConcurrentPlayers);
            MatchMakingPlayerJoinAcceptNetEventsPerPlayer = new CapacityDict<ushort, FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>>(maxConcurrentPlayers);
            PlayerTakeDamageNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayerTakeDamageNetEventS2C>>(maxConcurrentPlayers);
            PlayerDiedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayerDiedNetEventS2C>>(maxConcurrentPlayers);
            BulletDestroyedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<BulletDestroyedNetEventS2C>>(maxConcurrentPlayers);
            PlayerSwapNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayersSwapNetEventS2C>>(maxConcurrentPlayers);
            TalentCardObtainedNetEventsPerPlayer = new CapacityDict<ushort, FixedClassUnorderedList<TalentCardObtainedNetEventS2C>>(maxConcurrentPlayers);
            TalentCardHitNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<TalentCardHitNetEventS2C>>(maxConcurrentPlayers);
            PowerUpBallSpawnedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>>(maxConcurrentPlayers);
            PowerUpBallObtainedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>>(maxConcurrentPlayers);
            PlayerSwitchTeamNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>>(maxConcurrentPlayers);
            StartMatchCountdownNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<StartMatchCountdownNetEventS2C>>(maxConcurrentPlayers);
            StopMatchCountdownNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<StopMatchCountdownNetEventS2C>>(maxConcurrentPlayers);
            StartMatchEligibleChangedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>>(maxConcurrentPlayers);
            StageEndNetEventsPerPlayer = new CapacityDict<ushort, FixedClassUnorderedList<StageEndNetEventS2C>>(maxConcurrentPlayers);
            TeamLostNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<TeamLostNetEventS2C>>(maxConcurrentPlayers);

            _bulletSpawnListPool = new ConcurrentPool<FixedUnorderedList<BulletSpawnNetEventS2C>>(() => new FixedUnorderedList<BulletSpawnNetEventS2C>(networkConfig.MaxCap.BulletSpawnNetEvents), maxConcurrentPlayers);
            _playerRejoinAcceptListPool = new ConcurrentPool<FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>>(() =>
            {
                var list =new FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents, ()=>new PlayerRejoinAcceptPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount));
                list.Clear();
                return list;
            }, maxConcurrentPlayers);
            
            _matchMakingPlayerJoinAcceptListPool = new ConcurrentPool<FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>>(() =>
            {
                var list =new FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents, ()=>new MatchMakingPlayerJoinAcceptPacketS2C(networkConfig.MaxCap));
                list.Clear();
                return list;
            }, maxConcurrentPlayers);
            
            _playerTakeDamageListPool = new ConcurrentPool<FixedUnorderedList<PlayerTakeDamageNetEventS2C>>(() => new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(networkConfig.MaxCap.PlayerTakeDamageNetEvents), maxConcurrentPlayers);
            _playerDiedListPool = new ConcurrentPool<FixedUnorderedList<PlayerDiedNetEventS2C>>(() => new FixedUnorderedList<PlayerDiedNetEventS2C>(networkConfig.MaxCap.PlayerDiedNetEvents), maxConcurrentPlayers);
            _bulletDestroyedListPool = new ConcurrentPool<FixedUnorderedList<BulletDestroyedNetEventS2C>>(() => new FixedUnorderedList<BulletDestroyedNetEventS2C>(networkConfig.MaxCap.BulletDestroyedNetEvents), maxConcurrentPlayers);
            _playerSwapListPool= new ConcurrentPool<FixedUnorderedList<PlayersSwapNetEventS2C>>(() => new FixedUnorderedList<PlayersSwapNetEventS2C>(networkConfig.MaxCap.PlayerSwapNetEvents), maxConcurrentPlayers);
            _talentCardObtainedListPool = new ConcurrentPool<FixedClassUnorderedList<TalentCardObtainedNetEventS2C>>(() => new FixedClassUnorderedList<TalentCardObtainedNetEventS2C>(networkConfig.MaxCap.TalentCardObtainedNetEvent, ()=>new TalentCardObtainedNetEventS2C()), maxConcurrentPlayers);
            _talentCardHitListPool = new ConcurrentPool<FixedUnorderedList<TalentCardHitNetEventS2C>>(() => new FixedUnorderedList<TalentCardHitNetEventS2C>(networkConfig.MaxCap.TalentCardHitNetEvents), maxConcurrentPlayers);
            _powerUpBallsSpawnedListPool = new ConcurrentPool<FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>>(() => new FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>(networkConfig.MaxCap.PowerUpSpawnedNetEvents), maxConcurrentPlayers);
            _powerUpBallsObtainedListPool = new ConcurrentPool<FixedUnorderedList<PowerUpBallObtainedNetEventS2C>>(() => new FixedUnorderedList<PowerUpBallObtainedNetEventS2C>(networkConfig.MaxCap.PowerUpObtainedNetEvents), maxConcurrentPlayers);
            _playerSwitchTeamListPool = new ConcurrentPool<FixedUnorderedList<PlayerSwitchTeamNetEventS2C>>(() => new FixedUnorderedList<PlayerSwitchTeamNetEventS2C>(networkConfig.MaxCap.PlayerSwitchTeamNetEvents), maxConcurrentPlayers);
            _startMatchCountdownListPool = new ConcurrentPool<FixedUnorderedList<StartMatchCountdownNetEventS2C>>(() => new FixedUnorderedList<StartMatchCountdownNetEventS2C>(networkConfig.MaxCap.StartMatchCountdownNetEvents), maxConcurrentPlayers);
            _stopMatchCountdownListPool = new ConcurrentPool<FixedUnorderedList<StopMatchCountdownNetEventS2C>>(() => new FixedUnorderedList<StopMatchCountdownNetEventS2C>(networkConfig.MaxCap.StopMatchCountdownNetEvents), maxConcurrentPlayers);
            _startMatchEligibleChangedListPool = new ConcurrentPool<FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>>(() => new FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>(networkConfig.MaxCap.StartMatchEligibleChangedNetEvents), maxConcurrentPlayers);
            _stageEndNetEventsListPool = new ConcurrentPool<FixedClassUnorderedList<StageEndNetEventS2C>>(() =>
            {
                var list = new FixedClassUnorderedList<StageEndNetEventS2C>(networkConfig.MaxCap.StageEndNetEvents, () => new StageEndNetEventS2C(sharedGamePlayConfig.MaxTeamsAmount));
                list.Clear();
                return list;
            }, maxConcurrentPlayers);
            _teamLostNetEventsListPool = new ConcurrentPool<FixedUnorderedList<TeamLostNetEventS2C>>(() => new FixedUnorderedList<TeamLostNetEventS2C>(sharedGamePlayConfig.MaxTeamsAmount), maxConcurrentPlayers);
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

            if (!PlayerRejoinAcceptNetEventsPerPlayer.ContainsKey(playerId))
            {
                PlayerRejoinAcceptNetEventsPerPlayer.Add(playerId, _playerRejoinAcceptListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!MatchMakingPlayerJoinAcceptNetEventsPerPlayer.ContainsKey(playerId))
            {
                MatchMakingPlayerJoinAcceptNetEventsPerPlayer.Add(playerId, _matchMakingPlayerJoinAcceptListPool.Get());
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

            if (!PlayerDiedNetEventsPerPlayer.ContainsKey(playerId))
            {
                PlayerDiedNetEventsPerPlayer.Add(playerId, _playerDiedListPool.Get());
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

            if (!TalentCardObtainedNetEventsPerPlayer.ContainsKey(playerId))
            {
                TalentCardObtainedNetEventsPerPlayer.Add(playerId, _talentCardObtainedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!TalentCardHitNetEventsPerPlayer.ContainsKey(playerId))
            {
                TalentCardHitNetEventsPerPlayer.Add(playerId, _talentCardHitListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!PowerUpBallSpawnedNetEventsPerPlayer.ContainsKey(playerId))
            {
                PowerUpBallSpawnedNetEventsPerPlayer.Add(playerId, _powerUpBallsSpawnedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!PowerUpBallObtainedNetEventsPerPlayer.ContainsKey(playerId))
            {
                PowerUpBallObtainedNetEventsPerPlayer.Add(playerId, _powerUpBallsObtainedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!PlayerSwitchTeamNetEventsPerPlayer.ContainsKey(playerId))
            {
                PlayerSwitchTeamNetEventsPerPlayer.Add(playerId, _playerSwitchTeamListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!StartMatchCountdownNetEventsPerPlayer.ContainsKey(playerId))
            {
                StartMatchCountdownNetEventsPerPlayer.Add(playerId, _startMatchCountdownListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!StopMatchCountdownNetEventsPerPlayer.ContainsKey(playerId))
            {
                StopMatchCountdownNetEventsPerPlayer.Add(playerId, _stopMatchCountdownListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!StageEndNetEventsPerPlayer.ContainsKey(playerId))
            {
                StageEndNetEventsPerPlayer.Add(playerId, _stageEndNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!TeamLostNetEventsPerPlayer.ContainsKey(playerId))
            {
                TeamLostNetEventsPerPlayer.Add(playerId, _teamLostNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!StartMatchEligibleChangedNetEventsPerPlayer.ContainsKey(playerId))
            {
                StartMatchEligibleChangedNetEventsPerPlayer.Add(playerId, _startMatchEligibleChangedListPool.Get());
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
            var joinAcceptedList = PlayerRejoinAcceptNetEventsPerPlayer[playerId];
            joinAcceptedList.Clear();
            _playerRejoinAcceptListPool.Return(joinAcceptedList);
            var matchMakingJoinAcceptedList = MatchMakingPlayerJoinAcceptNetEventsPerPlayer[playerId];
            matchMakingJoinAcceptedList.Clear();
            _matchMakingPlayerJoinAcceptListPool.Return(matchMakingJoinAcceptedList);
            var playerTakeDamageedList = PlayerTakeDamageNetEventsPerPlayer[playerId];
            playerTakeDamageedList.Clear();
            _playerTakeDamageListPool.Return(playerTakeDamageedList);
            var playerDiedList = PlayerDiedNetEventsPerPlayer[playerId];
            playerDiedList.Clear();
            _playerDiedListPool.Return(playerDiedList);
            var bulletDestroyededList = BulletDestroyedNetEventsPerPlayer[playerId];
            bulletDestroyededList.Clear();
            _bulletDestroyedListPool.Return(bulletDestroyededList);
            var playerSwapList = PlayerSwapNetEventsPerPlayer[playerId];
            playerSwapList.Clear();
            _playerSwapListPool.Return(playerSwapList);
            var talentCardObtainedList = TalentCardObtainedNetEventsPerPlayer[playerId];
            talentCardObtainedList.Clear();
            _talentCardObtainedListPool.Return(talentCardObtainedList);
            var talentCardHitList = TalentCardHitNetEventsPerPlayer[playerId];
            talentCardHitList.Clear();
            _talentCardHitListPool.Return(talentCardHitList);
            var powerUpBallsSpawnedList = PowerUpBallSpawnedNetEventsPerPlayer[playerId];
            powerUpBallsSpawnedList.Clear();
            _powerUpBallsSpawnedListPool.Return(powerUpBallsSpawnedList);
            var powerUpBallsObtainedList = PowerUpBallObtainedNetEventsPerPlayer[playerId];
            powerUpBallsObtainedList.Clear();
            _powerUpBallsObtainedListPool.Return(powerUpBallsObtainedList);
            var playerSwitchTeamList = PlayerSwitchTeamNetEventsPerPlayer[playerId];
            playerSwitchTeamList.Clear();
            _playerSwitchTeamListPool.Return(playerSwitchTeamList);
            var startMatchCountdownList = StartMatchCountdownNetEventsPerPlayer[playerId];
            startMatchCountdownList.Clear();
            _startMatchCountdownListPool.Return(startMatchCountdownList);
            var stopMatchCountdownList = StopMatchCountdownNetEventsPerPlayer[playerId];
            stopMatchCountdownList.Clear();
            _stopMatchCountdownListPool.Return(stopMatchCountdownList);
            var stageEndList = StageEndNetEventsPerPlayer[playerId];
            stageEndList.Clear();
            _stageEndNetEventsListPool.Return(stageEndList);
            var teamLostList = TeamLostNetEventsPerPlayer[playerId];
            teamLostList.Clear();
            _teamLostNetEventsListPool.Return(teamLostList);
            var startMatchEligibleChangedList = StartMatchEligibleChangedNetEventsPerPlayer[playerId];
            startMatchEligibleChangedList.Clear();
            _startMatchEligibleChangedListPool.Return(startMatchEligibleChangedList);
        
            BulletSpawnNetEventsPerPlayer.Remove(playerId);
            PlayerRejoinAcceptNetEventsPerPlayer.Remove(playerId);
            MatchMakingPlayerJoinAcceptNetEventsPerPlayer.Remove(playerId);
            PlayerTakeDamageNetEventsPerPlayer.Remove(playerId);
            PlayerDiedNetEventsPerPlayer.Remove(playerId);
            BulletDestroyedNetEventsPerPlayer.Remove(playerId);
            PlayerSwapNetEventsPerPlayer.Remove(playerId);
            TalentCardObtainedNetEventsPerPlayer.Remove(playerId);
            TalentCardHitNetEventsPerPlayer.Remove(playerId);
            PowerUpBallSpawnedNetEventsPerPlayer.Remove(playerId);
            PowerUpBallObtainedNetEventsPerPlayer.Remove(playerId);
            PlayerSwitchTeamNetEventsPerPlayer.Remove(playerId);
            StartMatchCountdownNetEventsPerPlayer.Remove(playerId);
            StopMatchCountdownNetEventsPerPlayer.Remove(playerId);
            StageEndNetEventsPerPlayer.Remove(playerId);
            TeamLostNetEventsPerPlayer.Remove(playerId);
            StartMatchEligibleChangedNetEventsPerPlayer.Remove(playerId);
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

        public void AddPlayerDiedNetEvent(int onTick, ushort playerId, float maxShootCooldown, float shootCooldownSecondsLeft)
        {
            foreach (var kvp in PlayerDiedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.PlayerMaxShootCooldown = maxShootCooldown;
                packet.PlayerShootCooldownSecondsLeft = shootCooldownSecondsLeft;
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

        public void AddPlayerJoinAcceptedEvent(int onTick, PlayerStateS2C playerState, MatchSimulationStateS2C simulationState)
        {
            foreach (var kvp in PlayerRejoinAcceptNetEventsPerPlayer)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.IsLocal = playerState.Id == kvp.Key;
                packet.PlayerState = playerState;
                packet.SimulationState = simulationState;
            }
        }

        public void AddMatchMakingPlayerJoinAcceptedEvent(int onTick, MatchMakingPlayerStateS2C playerState, MatchMakingSimulationStateS2C simulationState)
        {
            foreach (var kvp in MatchMakingPlayerJoinAcceptNetEventsPerPlayer)
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

        public void AddTalentCardObtainedNetEvent(int onTick, ushort cardId, ushort obtainedByPlayerId, FixedOrderedList<TalentStateS2C> talents)
        {
            LogService.LogError($"Server Add talent card obtained! {cardId}");

            foreach (var kvp in TalentCardObtainedNetEventsPerPlayer)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.TalentCardId = cardId;
                packet.ObtainedByPlayerId = obtainedByPlayerId;
                packet.Talents = talents;
            }
        }

        public void AddTalentCardHitNetEvent(int onTick, ushort cardId, ushort cardHealth)
        {
            LogService.LogError("Server Add talent card hit!");
            foreach (var kvp in TalentCardHitNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.TalentCardId = cardId;
                packet.TalentCardHealth = cardHealth;
            }
        }

        public void AddPowerUpSpawnedNetEvent(int onTick, ushort powerUpBallId, Vector2 position)
        {
            foreach (var kvp in PowerUpBallSpawnedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PowerUpBallId = powerUpBallId;
                packet.Position = position;
            }
        }

        public void AddPowerUpObtainedNetEvent(int onTick, ushort powerUpBallId, ushort byPlayerId)
        {
            foreach (var kvp in PowerUpBallObtainedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.Id = powerUpBallId;
                packet.ObtainedByPlayerId = byPlayerId;
            }
        }

        public void AddPlayerSwitchTeamNetEvent(int onTick, ushort playerId, ushort teamId)
        {
            foreach (var kvp in PlayerSwitchTeamNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.TeamId = teamId;
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
            
            if (PlayerRejoinAcceptNetEventsPerPlayer.TryGetValue(playerId, out var joinAcceptNetEvents))
            {
                for (int i = joinAcceptNetEvents.Count - 1; i >= 0; i--)
                {
                    if(joinAcceptNetEvents[i].OccuredOnTick < tick)
                    {
                        joinAcceptNetEvents.RemoveAt(i);
                    }
                }
            } 
            
            if (MatchMakingPlayerJoinAcceptNetEventsPerPlayer.TryGetValue(playerId, out var makingPlayerJoinAcceptNetEvents))
            {
                for (int i = makingPlayerJoinAcceptNetEvents.Count - 1; i >= 0; i--)
                {
                    if(makingPlayerJoinAcceptNetEvents[i].OccuredOnTick < tick)
                    {
                        makingPlayerJoinAcceptNetEvents.RemoveAt(i);
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

            if (PlayerDiedNetEventsPerPlayer.TryGetValue(playerId, out var playerDiedNetEvents))
            {
                for (int i = playerDiedNetEvents.Count - 1; i >= 0; i--)
                {
                    if(playerDiedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerDiedNetEvents.RemoveAt(i);
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

            if (TalentCardObtainedNetEventsPerPlayer.TryGetValue(playerId, out var talentCardObtainedNetEvents))
            {
                for (int i = talentCardObtainedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (talentCardObtainedNetEvents[i].OccuredOnTick < tick)
                    {
                        talentCardObtainedNetEvents.RemoveAt(i);
                        LogService.LogError("Server remove talent card obtained!");
                    }
                }
            }
            
            if (TalentCardHitNetEventsPerPlayer.TryGetValue(playerId, out var talentCardHitNetEvents))
            {
                for (int i = talentCardHitNetEvents.Count - 1; i >= 0; i--)
                {
                    if (talentCardHitNetEvents[i].OccuredOnTick < tick)
                    {
                        talentCardHitNetEvents.RemoveAt(i);
                        LogService.LogError("Server remove talent card hit!");
                    }
                }
            }
            
            if (PlayerSwitchTeamNetEventsPerPlayer.TryGetValue(playerId, out var playerSwitchTeamNetEvents))
            {
                for (int i = playerSwitchTeamNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerSwitchTeamNetEvents[i].OccuredOnTick < tick)
                    {
                        playerSwitchTeamNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PowerUpBallSpawnedNetEventsPerPlayer.TryGetValue(playerId, out var powerUpBallSpawnedNetEvents))
            {
                for (int i = powerUpBallSpawnedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (powerUpBallSpawnedNetEvents[i].OccuredOnTick < tick)
                    {
                        powerUpBallSpawnedNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PowerUpBallObtainedNetEventsPerPlayer.TryGetValue(playerId, out var powerUpBallObtainedNetEvents))
            {
                for (int i = powerUpBallObtainedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (powerUpBallObtainedNetEvents[i].OccuredOnTick < tick)
                    {
                        powerUpBallObtainedNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (StartMatchCountdownNetEventsPerPlayer.TryGetValue(playerId, out var startMatchCountdownNetEvents))
            {
                for (int i = startMatchCountdownNetEvents.Count - 1; i >= 0; i--)
                {
                    if (startMatchCountdownNetEvents[i].OccuredOnTick < tick)
                    {
                        startMatchCountdownNetEvents.RemoveAt(i);
                    }
                }
            }

            if (StopMatchCountdownNetEventsPerPlayer.TryGetValue(playerId, out var stopMatchCountdownNetEvents))
            {
                for (int i = stopMatchCountdownNetEvents.Count - 1; i >= 0; i--)
                {
                    if (stopMatchCountdownNetEvents[i].OccuredOnTick < tick)
                    {
                        stopMatchCountdownNetEvents.RemoveAt(i);
                    }
                }
            }

            if (StageEndNetEventsPerPlayer.TryGetValue(playerId, out var stageEndNetEvents))
            {
                for (int i = stageEndNetEvents.Count - 1; i >= 0; i--)
                {
                    if (stageEndNetEvents[i].OccuredOnTick < tick)
                    {
                        stageEndNetEvents.RemoveAt(i);
                    }
                }
            }

            if (TeamLostNetEventsPerPlayer.TryGetValue(playerId, out var teamLostNetEvents))
            {
                for (int i = teamLostNetEvents.Count - 1; i >= 0; i--)
                {
                    if (teamLostNetEvents[i].OccuredOnTick < tick)
                    {
                        teamLostNetEvents.RemoveAt(i);
                    }
                }
            }

            if (StartMatchEligibleChangedNetEventsPerPlayer.TryGetValue(playerId, out var startMatchEligibleChangedNetEvents))
            {
                for (int i = startMatchEligibleChangedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (startMatchEligibleChangedNetEvents[i].OccuredOnTick < tick)
                    {
                        startMatchEligibleChangedNetEvents.RemoveAt(i);
                    }
                }
            }
        }

        public void AddStartMatchCountdownNetEvent(int onTick, ushort seconds)
        {
            foreach (var kvp in StartMatchCountdownNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CountdownSeconds = seconds;
            }
        }

        public void AddStopMatchCountdownNetEvent(int onTick)
        {
            foreach (var kvp in StopMatchCountdownNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
            }
        }

        public void AddStartMatchEligibleChangedNetEvent(int onTick, bool isEligible)
        {
            foreach (var kvp in StartMatchEligibleChangedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.IsEligible = isEligible;
            }
        }

        public void AddStageEndNetEvent(int onTick, ushort winningTeamId, Dictionary<ushort, int> jemsWon, Dictionary<ushort, int> totalJems)
        {
            foreach (var kvp in StageEndNetEventsPerPlayer)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.WinningTeamId = winningTeamId;
                packet.JemsWonPerTeam.Clear();
                foreach (var jems in jemsWon)
                {
                    packet.JemsWonPerTeam.Add(jems.Key, jems.Value);
                }
                packet.TotalJemsPerTeam.Clear();
                foreach (var jems in totalJems)
                {
                    packet.TotalJemsPerTeam.Add(jems.Key, jems.Value);
                }
            }
        }

        public void AddTeamLostNetEvent(int onTick, ushort losingTeamId, Dictionary<ushort, int> totalGemsPerTeam, Dictionary<ushort, int> gemsGainedPerTeam)
        {
            foreach (var kvp in TeamLostNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.LosingTeamId = losingTeamId;
                packet.TotalGemsPerTeam = totalGemsPerTeam;
                packet.GemsGainedPerTeam = gemsGainedPerTeam;
            }
        }
    }
}