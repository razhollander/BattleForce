using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
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
        public CapacityDict<ushort, FixedUnorderedList<TalentSwitchNetEventS2C>> TalentSwitchNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<GainBoltsNetEventS2C>> GainBoltsNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>> EnvironmentSpringPlayerCollisionNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>> PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<PreparationPhaseEndedNetEventS2C>> PreparationPhaseEndedNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<CreateSwapFieldNetEventS2C>> CreateSwapFieldNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>> CreateKOProjectileNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> KOProjectHitPlayerNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<DeactivateKOTalentNetEventS2C>> DeactivateKOTalentNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<PerformDashPulseNetEventS2C>> PerformDashPulseNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>> ActivateSentryGunTalentNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>> DeactivateSentryGunTalentNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>> UpdatePlayerTalentStocksNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>> PlayerMaxShootCooldownChangedNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>> CreateGrapplingHookProjectileNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<GrapplingHookHitWallNetEventS2C>> GrapplingHookHitWallNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>> DeactivateGrapplingHookTalentNetEventsPerPlayer { get; }
        public CapacityDict<ushort, FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>> CreateMagneticPullFieldNetEventsPerPlayer { get; }

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
        private readonly ConcurrentPool<FixedUnorderedList<TalentSwitchNetEventS2C>> _talentSwitchNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>> _environmentSpringPlayerCollisionListPool;
        private readonly ConcurrentPool<FixedUnorderedList<GainBoltsNetEventS2C>> _gainBoltsNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>> _playerToEnvironmentTeleportGateCollisionListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PreparationPhaseEndedNetEventS2C>> _preparationPhaseEndedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<CreateSwapFieldNetEventS2C>> _createSwapFieldNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> _deactivateSwapTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<CreateKOProjectileNetEventS2C>> _createKOProjectileNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> _koProjectHitPlayerNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateKOTalentNetEventS2C>> _deactivateKOTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PerformDashPulseNetEventS2C>> _performDashPulseNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>> _activateSentryGunTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>> _deactivateSentryGunTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>> _updatePlayerTalentStocksNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>> _playerMaxShootCooldownChangedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>> _createGrapplingHookProjectileNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<GrapplingHookHitWallNetEventS2C>> _grapplingHookHitWallNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>> _deactivateGrapplingHookTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>> _createMagneticPullFieldNetEventsListPool;

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
            TalentSwitchNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<TalentSwitchNetEventS2C>>(maxConcurrentPlayers);
            EnvironmentSpringPlayerCollisionNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>>(maxConcurrentPlayers);
            GainBoltsNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<GainBoltsNetEventS2C>>(maxConcurrentPlayers);
            PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>>(maxConcurrentPlayers);
            PreparationPhaseEndedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PreparationPhaseEndedNetEventS2C>>(maxConcurrentPlayers);
            CreateSwapFieldNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<CreateSwapFieldNetEventS2C>>(maxConcurrentPlayers);
            DeactivateSwapTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(maxConcurrentPlayers);
            CreateKOProjectileNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>>(maxConcurrentPlayers);
            KOProjectHitPlayerNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(maxConcurrentPlayers);
            DeactivateKOTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(maxConcurrentPlayers);
            ActivateSentryGunTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>>(maxConcurrentPlayers);
            DeactivateSentryGunTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>>(maxConcurrentPlayers);
            PerformDashPulseNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PerformDashPulseNetEventS2C>>(maxConcurrentPlayers);
            UpdatePlayerTalentStocksNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>>(maxConcurrentPlayers);
            PlayerMaxShootCooldownChangedNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>>(maxConcurrentPlayers);
            CreateGrapplingHookProjectileNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>>(maxConcurrentPlayers);
            GrapplingHookHitWallNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<GrapplingHookHitWallNetEventS2C>>(maxConcurrentPlayers);
            DeactivateGrapplingHookTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>>(maxConcurrentPlayers);
            CreateMagneticPullFieldNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>>(maxConcurrentPlayers);
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
            _talentSwitchNetEventsListPool = new ConcurrentPool<FixedUnorderedList<TalentSwitchNetEventS2C>>(() => new FixedUnorderedList<TalentSwitchNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);
            _environmentSpringPlayerCollisionListPool = new ConcurrentPool<FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>>(() => new FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>(networkConfig.MaxCap.EnvironmentSpringPlayerCollisionNetEvents), maxConcurrentPlayers);
            _gainBoltsNetEventsListPool = new ConcurrentPool<FixedUnorderedList<GainBoltsNetEventS2C>>(() => new FixedUnorderedList<GainBoltsNetEventS2C>(networkConfig.MaxCap.GainBoltsNetEvents), maxConcurrentPlayers);
            _playerToEnvironmentTeleportGateCollisionListPool = new ConcurrentPool<FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>>(() => new FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>(networkConfig.MaxCap.PlayerToEnvironmentTeleportGateCollisionNetEvents), maxConcurrentPlayers);
            _preparationPhaseEndedListPool = new ConcurrentPool<FixedUnorderedList<PreparationPhaseEndedNetEventS2C>>(() => new FixedUnorderedList<PreparationPhaseEndedNetEventS2C>(networkConfig.MaxCap.PreparationPhaseEndedNetEvents), maxConcurrentPlayers);
            _createSwapFieldNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateSwapFieldNetEventS2C>>(() => new FixedUnorderedList<CreateSwapFieldNetEventS2C>(networkConfig.MaxCap.CreateSwapFieldNetEvents), maxConcurrentPlayers);
            _deactivateSwapTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateSwapTalentNetEventS2C>(networkConfig.MaxCap.DestroySwapFieldNetEvents), maxConcurrentPlayers);
            _createKOProjectileNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateKOProjectileNetEventS2C>>(() => new FixedUnorderedList<CreateKOProjectileNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);
            _koProjectHitPlayerNetEventsListPool = new ConcurrentPool<FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(() => new FixedUnorderedList<KOProjectHitPlayerNetEventS2C>(networkConfig.MaxCap.KOProjectHitPlayerNetEvents), maxConcurrentPlayers);
            _deactivateKOTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateKOTalentNetEventS2C>(networkConfig.MaxCap.DeactivateKOTalentNetEvents), maxConcurrentPlayers);
            _performDashPulseNetEventsListPool = new ConcurrentPool<FixedUnorderedList<PerformDashPulseNetEventS2C>>(() => new FixedUnorderedList<PerformDashPulseNetEventS2C>(networkConfig.MaxCap.PerformDashPulseNetEvents), maxConcurrentPlayers);
            _activateSentryGunTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>>(() => new FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>(networkConfig.MaxCap.ActivateSentryGunTalentNetEvents), maxConcurrentPlayers);
            _deactivateSentryGunTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>(networkConfig.MaxCap.DeactivateSentryGunTalentNetEvents), maxConcurrentPlayers);
            _updatePlayerTalentStocksNetEventsListPool = new ConcurrentPool<FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>>(() => new FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>(networkConfig.MaxCap.UpdatePlayerTalentStocksNetEvent), maxConcurrentPlayers);
            _playerMaxShootCooldownChangedListPool = new ConcurrentPool<FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>>(() => new FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>(networkConfig.MaxCap.PlayerMaxShootCooldownChangedNetEvents), maxConcurrentPlayers);
            _createGrapplingHookProjectileNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>>(() => new FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>(networkConfig.MaxCap.CreateGrapplingHookProjectileNetEvents), maxConcurrentPlayers);
            _grapplingHookHitWallNetEventsListPool = new ConcurrentPool<FixedUnorderedList<GrapplingHookHitWallNetEventS2C>>(() => new FixedUnorderedList<GrapplingHookHitWallNetEventS2C>(networkConfig.MaxCap.GrapplingHookHitWallNetEvents), maxConcurrentPlayers);
            _deactivateGrapplingHookTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>(networkConfig.MaxCap.DeactivateGrapplingHookTalentNetEvents), maxConcurrentPlayers);
            _createMagneticPullFieldNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>>(() => new FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>(networkConfig.MaxCap.CreateMagneticPullFieldNetEvents), maxConcurrentPlayers);
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

            if (!TalentSwitchNetEventsPerPlayer.ContainsKey(playerId))
            {
                TalentSwitchNetEventsPerPlayer.Add(playerId, _talentSwitchNetEventsListPool.Get());
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

            if (!EnvironmentSpringPlayerCollisionNetEventsPerPlayer.ContainsKey(playerId))
            {
                EnvironmentSpringPlayerCollisionNetEventsPerPlayer.Add(playerId, _environmentSpringPlayerCollisionListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!GainBoltsNetEventsPerPlayer.ContainsKey(playerId))
            {
                GainBoltsNetEventsPerPlayer.Add(playerId, _gainBoltsNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer.ContainsKey(playerId))
            {
                PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer.Add(playerId, _playerToEnvironmentTeleportGateCollisionListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!PreparationPhaseEndedNetEventsPerPlayer.ContainsKey(playerId))
            {
                PreparationPhaseEndedNetEventsPerPlayer.Add(playerId, _preparationPhaseEndedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {playerId}");
            }

            if (!CreateSwapFieldNetEventsPerPlayer.ContainsKey(playerId))
            {
                CreateSwapFieldNetEventsPerPlayer.Add(playerId, _createSwapFieldNetEventsListPool.Get());
            }
            if (!DeactivateSwapTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                DeactivateSwapTalentNetEventsPerPlayer.Add(playerId, _deactivateSwapTalentNetEventsListPool.Get());
            }
            if (!CreateKOProjectileNetEventsPerPlayer.ContainsKey(playerId))
            {
                CreateKOProjectileNetEventsPerPlayer.Add(playerId, _createKOProjectileNetEventsListPool.Get());
            }
            if (!KOProjectHitPlayerNetEventsPerPlayer.ContainsKey(playerId))
            {
                KOProjectHitPlayerNetEventsPerPlayer.Add(playerId, _koProjectHitPlayerNetEventsListPool.Get());
            }
            if (!DeactivateKOTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                DeactivateKOTalentNetEventsPerPlayer.Add(playerId, _deactivateKOTalentNetEventsListPool.Get());
            }
            if (!PerformDashPulseNetEventsPerPlayer.ContainsKey(playerId))
            {
                PerformDashPulseNetEventsPerPlayer.Add(playerId, _performDashPulseNetEventsListPool.Get());
            }
            if (!ActivateSentryGunTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                ActivateSentryGunTalentNetEventsPerPlayer.Add(playerId, _activateSentryGunTalentNetEventsListPool.Get());
            }
            if (!DeactivateSentryGunTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                DeactivateSentryGunTalentNetEventsPerPlayer.Add(playerId, _deactivateSentryGunTalentNetEventsListPool.Get());
            }
            if (!UpdatePlayerTalentStocksNetEventsPerPlayer.ContainsKey(playerId))
            {
                UpdatePlayerTalentStocksNetEventsPerPlayer.Add(playerId, _updatePlayerTalentStocksNetEventsListPool.Get());
            }
            if (!PlayerMaxShootCooldownChangedNetEventsPerPlayer.ContainsKey(playerId))
            {
                PlayerMaxShootCooldownChangedNetEventsPerPlayer.Add(playerId, _playerMaxShootCooldownChangedListPool.Get());
            }
            if (!CreateGrapplingHookProjectileNetEventsPerPlayer.ContainsKey(playerId))
            {
                CreateGrapplingHookProjectileNetEventsPerPlayer.Add(playerId, _createGrapplingHookProjectileNetEventsListPool.Get());
            }
            if (!GrapplingHookHitWallNetEventsPerPlayer.ContainsKey(playerId))
            {
                GrapplingHookHitWallNetEventsPerPlayer.Add(playerId, _grapplingHookHitWallNetEventsListPool.Get());
            }
            if (!DeactivateGrapplingHookTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                DeactivateGrapplingHookTalentNetEventsPerPlayer.Add(playerId, _deactivateGrapplingHookTalentNetEventsListPool.Get());
            }
            if (!CreateMagneticPullFieldNetEventsPerPlayer.ContainsKey(playerId))
            {
                CreateMagneticPullFieldNetEventsPerPlayer.Add(playerId, _createMagneticPullFieldNetEventsListPool.Get());
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
            var talentSwitchList = TalentSwitchNetEventsPerPlayer[playerId];
            talentSwitchList.Clear();
            _talentSwitchNetEventsListPool.Return(talentSwitchList);
            var startMatchEligibleChangedList = StartMatchEligibleChangedNetEventsPerPlayer[playerId];
            startMatchEligibleChangedList.Clear();
            _startMatchEligibleChangedListPool.Return(startMatchEligibleChangedList);
            var environmentSpringPlayerCollisionList = EnvironmentSpringPlayerCollisionNetEventsPerPlayer[playerId];
            environmentSpringPlayerCollisionList.Clear();
            _environmentSpringPlayerCollisionListPool.Return(environmentSpringPlayerCollisionList);
            var gainBoltsList = GainBoltsNetEventsPerPlayer[playerId];
            gainBoltsList.Clear();
            _gainBoltsNetEventsListPool.Return(gainBoltsList);
            var playerToEnvironmentTeleportGateCollisionList = PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer[playerId];
            playerToEnvironmentTeleportGateCollisionList.Clear();
            _playerToEnvironmentTeleportGateCollisionListPool.Return(playerToEnvironmentTeleportGateCollisionList);
            var preparationPhaseEndedList = PreparationPhaseEndedNetEventsPerPlayer[playerId];
            preparationPhaseEndedList.Clear();
            _preparationPhaseEndedListPool.Return(preparationPhaseEndedList);
            var createSwapFieldNetEventsList = CreateSwapFieldNetEventsPerPlayer[playerId];
            createSwapFieldNetEventsList.Clear();
            _createSwapFieldNetEventsListPool.Return(createSwapFieldNetEventsList);
            var deactivateSwapTalentNetEventsList = DeactivateSwapTalentNetEventsPerPlayer[playerId];
            deactivateSwapTalentNetEventsList.Clear();
            _deactivateSwapTalentNetEventsListPool.Return(deactivateSwapTalentNetEventsList);

            var createKOProjectileNetEventsList = CreateKOProjectileNetEventsPerPlayer[playerId];
            createKOProjectileNetEventsList.Clear();
            _createKOProjectileNetEventsListPool.Return(createKOProjectileNetEventsList);

            var koProjectHitPlayerNetEventsList = KOProjectHitPlayerNetEventsPerPlayer[playerId];
            koProjectHitPlayerNetEventsList.Clear();
            _koProjectHitPlayerNetEventsListPool.Return(koProjectHitPlayerNetEventsList);

            var deactivateKOTalentNetEventsList = DeactivateKOTalentNetEventsPerPlayer[playerId];
            deactivateKOTalentNetEventsList.Clear();
            _deactivateKOTalentNetEventsListPool.Return(deactivateKOTalentNetEventsList);
            
            var performDashPulseNetEventsList = PerformDashPulseNetEventsPerPlayer[playerId];
            performDashPulseNetEventsList.Clear();
            _performDashPulseNetEventsListPool.Return(performDashPulseNetEventsList);

            var activateSentryGunTalentNetEventsList = ActivateSentryGunTalentNetEventsPerPlayer[playerId];
            activateSentryGunTalentNetEventsList.Clear();
            _activateSentryGunTalentNetEventsListPool.Return(activateSentryGunTalentNetEventsList);

            var deactivateSentryGunTalentNetEventsList = DeactivateSentryGunTalentNetEventsPerPlayer[playerId];
            deactivateSentryGunTalentNetEventsList.Clear();
            _deactivateSentryGunTalentNetEventsListPool.Return(deactivateSentryGunTalentNetEventsList);
            
            var updatePlayerTalentStocksNetEventsList = UpdatePlayerTalentStocksNetEventsPerPlayer[playerId];
            updatePlayerTalentStocksNetEventsList.Clear();
            _updatePlayerTalentStocksNetEventsListPool.Return(updatePlayerTalentStocksNetEventsList);
            
            var playerMaxShootCooldownChangedList = PlayerMaxShootCooldownChangedNetEventsPerPlayer[playerId];
            playerMaxShootCooldownChangedList.Clear();
            _playerMaxShootCooldownChangedListPool.Return(playerMaxShootCooldownChangedList);

            var createGrapplingHookProjectileNetEventsList = CreateGrapplingHookProjectileNetEventsPerPlayer[playerId];
            createGrapplingHookProjectileNetEventsList.Clear();
            _createGrapplingHookProjectileNetEventsListPool.Return(createGrapplingHookProjectileNetEventsList);

            var grapplingHookHitWallNetEventsList = GrapplingHookHitWallNetEventsPerPlayer[playerId];
            grapplingHookHitWallNetEventsList.Clear();
            _grapplingHookHitWallNetEventsListPool.Return(grapplingHookHitWallNetEventsList);

            var deactivateGrapplingHookTalentNetEventsList = DeactivateGrapplingHookTalentNetEventsPerPlayer[playerId];
            deactivateGrapplingHookTalentNetEventsList.Clear();
            _deactivateGrapplingHookTalentNetEventsListPool.Return(deactivateGrapplingHookTalentNetEventsList);

            var createMagneticPullFieldNetEventsList = CreateMagneticPullFieldNetEventsPerPlayer[playerId];
            createMagneticPullFieldNetEventsList.Clear();
            _createMagneticPullFieldNetEventsListPool.Return(createMagneticPullFieldNetEventsList);

            CreateMagneticPullFieldNetEventsPerPlayer.Remove(playerId);
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
            TalentSwitchNetEventsPerPlayer.Remove(playerId);
            StartMatchEligibleChangedNetEventsPerPlayer.Remove(playerId);
            EnvironmentSpringPlayerCollisionNetEventsPerPlayer.Remove(playerId);
            GainBoltsNetEventsPerPlayer.Remove(playerId);
            PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer.Remove(playerId);
            PreparationPhaseEndedNetEventsPerPlayer.Remove(playerId);
            CreateSwapFieldNetEventsPerPlayer.Remove(playerId);
            DeactivateSwapTalentNetEventsPerPlayer.Remove(playerId);
            CreateKOProjectileNetEventsPerPlayer.Remove(playerId);
            KOProjectHitPlayerNetEventsPerPlayer.Remove(playerId);
            DeactivateKOTalentNetEventsPerPlayer.Remove(playerId);
            PerformDashPulseNetEventsPerPlayer.Remove(playerId);
            ActivateSentryGunTalentNetEventsPerPlayer.Remove(playerId);
            DeactivateSentryGunTalentNetEventsPerPlayer.Remove(playerId);
            UpdatePlayerTalentStocksNetEventsPerPlayer.Remove(playerId);
            PlayerMaxShootCooldownChangedNetEventsPerPlayer.Remove(playerId);
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

        public void AddPlayerDiedNetEvent(int onTick, ushort playerId)
        {
            foreach (var kvp in PlayerDiedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
            }
        }

        public void AddPlayerMaxShootCooldownChangedNetEvent(int onTick, ushort playerId, float maxShootCooldown, float shootCooldownSecondsLeft)
        {
            foreach (var kvp in PlayerMaxShootCooldownChangedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.MaxShootCooldown = maxShootCooldown;
                packet.ShootCooldownSecondsLeft = shootCooldownSecondsLeft;
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

        public void AddPlayersSwapEvent(int onTick, ushort casterPlayerId, ushort otherPlayerId, Vector2 casterPlayerPosition, Vector2 otherPlayerPosition, Vector2 casterPlayerDirection,
            Vector2 otherPlayerDirection)
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

        public void AddTalentCardObtainedNetEvent(int onTick, ushort cardId, ushort obtainedByPlayerId, FixedOrderedList<TalentStateS2C> playerTalents, bool didReplaceTalent)
        {
            foreach (var kvp in TalentCardObtainedNetEventsPerPlayer)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.TalentCardId = cardId;
                packet.ObtainedByPlayerId = obtainedByPlayerId;
                packet.PlayerTalents = playerTalents;
                packet.DidReplaceTalent = didReplaceTalent;
            }
        }

        public void AddTalentCardHitNetEvent(int onTick, ushort cardId, ushort cardHealth)
        {
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

            if (TalentSwitchNetEventsPerPlayer.TryGetValue(playerId, out var talentSwitchNetEvents))
            {
                for (int i = talentSwitchNetEvents.Count - 1; i >= 0; i--)
                {
                    if (talentSwitchNetEvents[i].OccuredOnTick < tick)
                    {
                        talentSwitchNetEvents.RemoveAt(i);
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

            if (EnvironmentSpringPlayerCollisionNetEventsPerPlayer.TryGetValue(playerId, out var environmentSpringPlayerCollisionNetEvents))
            {
                for (int i = environmentSpringPlayerCollisionNetEvents.Count - 1; i >= 0; i--)
                {
                    if (environmentSpringPlayerCollisionNetEvents[i].OccuredOnTick < tick)
                    {
                        environmentSpringPlayerCollisionNetEvents.RemoveAt(i);
                    }
                }
            }

            if (GainBoltsNetEventsPerPlayer.TryGetValue(playerId, out var gainBoltsNetEvents))
            {
                for (int i = gainBoltsNetEvents.Count - 1; i >= 0; i--)
                {
                    if (gainBoltsNetEvents[i].OccuredOnTick < tick)
                    {
                        gainBoltsNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer.TryGetValue(playerId, out var playerToEnvironmentTeleportGateCollisionNetEvents))
            {
                for (int i = playerToEnvironmentTeleportGateCollisionNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerToEnvironmentTeleportGateCollisionNetEvents[i].OccuredOnTick < tick)
                    {
                        playerToEnvironmentTeleportGateCollisionNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PreparationPhaseEndedNetEventsPerPlayer.TryGetValue(playerId, out var preparationPhaseEndedNetEvents))
            {
                for (int i = preparationPhaseEndedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (preparationPhaseEndedNetEvents[i].OccuredOnTick < tick)
                    {
                        preparationPhaseEndedNetEvents.RemoveAt(i);
                    }
                }
            }
            if (CreateSwapFieldNetEventsPerPlayer.TryGetValue(playerId, out var createSwapFieldNetEvents))
            {
                for (int i = createSwapFieldNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createSwapFieldNetEvents[i].OccuredOnTick < tick)
                    {
                        createSwapFieldNetEvents.RemoveAt(i);
                    }
                }
            }
            if (DeactivateSwapTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateSwapTalentNetEvents))
            {
                for (int i = deactivateSwapTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateSwapTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateSwapTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (CreateKOProjectileNetEventsPerPlayer.TryGetValue(playerId, out var createKOProjectileNetEvents))
            {
                for (int i = createKOProjectileNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createKOProjectileNetEvents[i].OccuredOnTick < tick)
                    {
                        createKOProjectileNetEvents.RemoveAt(i);
                    }
                }
            }
            if (KOProjectHitPlayerNetEventsPerPlayer.TryGetValue(playerId, out var koProjectHitPlayerNetEvents))
            {
                for (int i = koProjectHitPlayerNetEvents.Count - 1; i >= 0; i--)
                {
                    if (koProjectHitPlayerNetEvents[i].OccuredOnTick < tick)
                    {
                        koProjectHitPlayerNetEvents.RemoveAt(i);
                    }
                }
            }

if (DeactivateKOTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateKOTalentNetEvents))
            {
                for (int i = deactivateKOTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateKOTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateKOTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (PerformDashPulseNetEventsPerPlayer.TryGetValue(playerId, out var performDashPulseNetEvents))
            {
                for (int i = performDashPulseNetEvents.Count - 1; i >= 0; i--)
                {
                    if (performDashPulseNetEvents[i].OccuredOnTick < tick)
                    {
                        performDashPulseNetEvents.RemoveAt(i);
                    }
                }
            }
            if (ActivateSentryGunTalentNetEventsPerPlayer.TryGetValue(playerId, out var activateSentryGunTalentNetEvents))
            {
                for (int i = activateSentryGunTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateSentryGunTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        activateSentryGunTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (DeactivateSentryGunTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateSentryGunTalentNetEvents))
            {
                for (int i = deactivateSentryGunTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateSentryGunTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateSentryGunTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (UpdatePlayerTalentStocksNetEventsPerPlayer.TryGetValue(playerId, out var updatePlayerTalentsStocksNetEvnets))
            {
                for (int i = updatePlayerTalentsStocksNetEvnets.Count - 1; i >= 0; i--)
                {
                    if (updatePlayerTalentsStocksNetEvnets[i].OccuredOnTick < tick)
                    {
                        updatePlayerTalentsStocksNetEvnets.RemoveAt(i);
                    }
                }
            }
            if (PlayerMaxShootCooldownChangedNetEventsPerPlayer.TryGetValue(playerId, out var playerMaxShootCooldownChangedNetEvents))
            {
                for (int i = playerMaxShootCooldownChangedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerMaxShootCooldownChangedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerMaxShootCooldownChangedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (CreateGrapplingHookProjectileNetEventsPerPlayer.TryGetValue(playerId, out var createGrapplingHookProjectileNetEvents))
            {
                for (int i = createGrapplingHookProjectileNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createGrapplingHookProjectileNetEvents[i].OccuredOnTick < tick)
                    {
                        createGrapplingHookProjectileNetEvents.RemoveAt(i);
                    }
                }
            }

            if (GrapplingHookHitWallNetEventsPerPlayer.TryGetValue(playerId, out var grapplingHookHitWallNetEvents))
            {
                for (int i = grapplingHookHitWallNetEvents.Count - 1; i >= 0; i--)
                {
                    if (grapplingHookHitWallNetEvents[i].OccuredOnTick < tick)
                    {
                        grapplingHookHitWallNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateGrapplingHookTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateGrapplingHookTalentNetEvents))
            {
                for (int i = deactivateGrapplingHookTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateGrapplingHookTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateGrapplingHookTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (CreateMagneticPullFieldNetEventsPerPlayer.TryGetValue(playerId, out var createMagneticPullFieldNetEvents))
            {
                for (int i = createMagneticPullFieldNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createMagneticPullFieldNetEvents[i].OccuredOnTick < tick)
                    {
                        createMagneticPullFieldNetEvents.RemoveAt(i);
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

        public void AddTalentSwitchNetEvent(int onTick, ushort playerId, int newTalentIndex)
        {
            foreach (var kvp in TalentSwitchNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.NewTalentIndex = newTalentIndex;
            }
        }

        public void AddEnvironmentSpringPlayerCollisionNetEvent(int onTick, ushort springId, ushort playerId, Vector2 newPlayerDirection)
        {
            foreach (var kvp in EnvironmentSpringPlayerCollisionNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.SpringId = springId;
                packet.PlayerId = playerId;
                packet.NewPlayerDirection = newPlayerDirection;
            }
        }

        public void AddGainBoltsNetEvent(int onTick, ushort playerId, int gainedAmount, int totalTeamBolts)
        {
            foreach (var kvp in GainBoltsNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.GainedAmount = gainedAmount;
                packet.TotalTeamBolts = totalTeamBolts;
            }
        }

        public void AddPlayerToEnvironmentTeleportGateCollisionNetEvent(int onTick, ushort teleportPairId, Vector2 enterPoint, Vector2 exitPoint, ushort playerId)
        {
            foreach (var kvp in PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.PlayerId = playerId;
                packet.OccuredOnTick = onTick;
                packet.TeleportGatePairId = teleportPairId;
                packet.EnterPoint = enterPoint;
                packet.ExitPoint = exitPoint;
            }
        }

        public void AddPreparationPhaseEndedNetEvent(int onTick)
        {
            foreach (var kvp in PreparationPhaseEndedNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
            }
        }

        public void AddCreateSwapFieldNetEvent(int onTick, ushort swapFieldId, ushort casterPlayerId, int fieldEndTick, float maxRadius)
        {
            foreach (var kvp in CreateSwapFieldNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.SwapFieldId = swapFieldId;
                packet.CasterPlayerId = casterPlayerId;
                packet.EndOnTick = fieldEndTick;
                packet.MaxRadius = maxRadius;
            }
        }

        public void AddDeactivateSwapTalentNetEvent(int onTick, ushort casterPlayerId, ushort swapFieldId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateSwapTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.SwapFieldId = swapFieldId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }
        public void AddCreateKOProjectileNetEvent(int onTick, ushort projectileId, ushort casterPlayerId, System.Numerics.Vector2 position, System.Numerics.Vector2 velocity, float size)
        {
            foreach (var kvp in CreateKOProjectileNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.KoProjectile.Id = projectileId;
                packet.KoProjectile.PlayerCasterId = casterPlayerId;
                packet.KoProjectile.Position = position;
                packet.KoProjectile.Velocity = velocity;
                packet.KoProjectile.Size = size;
            }
        }

        public void AddKOProjectHitPlayerNetEvent(int onTick, ushort projectileId, ushort hitPlayerId, System.Numerics.Vector2 hitPosition)
        {
            foreach (var kvp in KOProjectHitPlayerNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.ProjectileId = projectileId;
                packet.HitPlayerId = hitPlayerId;
                packet.HitPosition = hitPosition;
            }
        }

        public void AddDeactivateKOTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateKOTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.KoProjectileId = projectileId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }
        
        public void AddPerformDashPulseNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in PerformDashPulseNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddUpdatePlayerTalentStocksNetEventS2C(int onTick, ushort casterPlayerId, TalentType talentType, int currentStocksAmount, int recieveNextStockOnTick)
        {
            foreach (var kvp in UpdatePlayerTalentStocksNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentType = talentType;
                packet.CurrentStocksAmount = currentStocksAmount;
                packet.RecieveNextStockOnTick = recieveNextStockOnTick;
            }
        }

        public void AddActivateSentryGunTalentNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in ActivateSentryGunTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddDeactivateSentryGunTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateSentryGunTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddCreateGrapplingHookProjectileNetEvent(int onTick, ushort projectileId, ushort playerCasterId, System.Numerics.Vector2 position, System.Numerics.Vector2 velocity, float size)
        {
            foreach (var kvp in CreateGrapplingHookProjectileNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.GrapplingHookProjectile.Id = projectileId;
                packet.GrapplingHookProjectile.PlayerCasterId = playerCasterId;
                packet.GrapplingHookProjectile.StartPosition = position;
                packet.GrapplingHookProjectile.Position = position;
                packet.GrapplingHookProjectile.Velocity = velocity;
                packet.GrapplingHookProjectile.Size = size;
                packet.GrapplingHookProjectile.CreatedOnTick = onTick;
            }
        }

        public void AddGrapplingHookHitWallNetEvent(int onTick, ushort projectileId, ushort hitWallId, System.Numerics.Vector2 hitPosition)
        {
            foreach (var kvp in GrapplingHookHitWallNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.ProjectileId = projectileId;
                packet.HitWallId = hitWallId;
                packet.HitPosition = hitPosition;
            }
        }

        public void AddDeactivateGrapplingHookTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateGrapplingHookTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.ProjectileId = projectileId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddCreateMagneticPullFieldNetEventS2C(int onTick, ushort casterPlayerId, Vector2 direction, int talentCooldownEndTick, ushort hitEnemyId)
        {
            foreach (var kvp in CreateMagneticPullFieldNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.Direction = direction;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
                packet.HitEnemyId = hitEnemyId;
            }
        }
    }
}
