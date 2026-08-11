using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NetEventsDataService : INetEventsDataService
    {
        public CapacityDict<long, FixedUnorderedList<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerClient { get; private set; } // todo: remove events related to bullet when bullet is destroyed
        public CapacityDict<long, FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>> PlayerRejoinAcceptNetEventsPerClient { get; private set; } // todo: remove events related to player when player is destroyed
        public CapacityDict<long, FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>> MatchMakingPlayerJoinAcceptNetEventsPerClient { get; private set; } // todo: remove events related to player when player is destroyed
        public CapacityDict<long, FixedUnorderedList<PlayerTakeDamageNetEventS2C>> PlayerTakeDamageNetEventsPerClient { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<long, FixedUnorderedList<PlayerDiedNetEventS2C>> PlayerDiedNetEventsPerClient { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<long, FixedUnorderedList<BulletDestroyedNetEventS2C>> BulletDestroyedNetEventsPerClient { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<long, FixedUnorderedList<PlayersSwapNetEventS2C>> PlayerSwapNetEventsPerClient { get; private set;} // todo: remove events related to player hit when player is destroyed
        public CapacityDict<long, FixedClassUnorderedList<TalentCardObtainedNetEventS2C>> TalentCardObtainedNetEventsPerClient { get; private set; } // todo: remove events related to player hit when player is destroyed
        public CapacityDict<long, FixedUnorderedList<TalentCardHitNetEventS2C>> TalentCardHitNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>> PlayerSpinnedStartedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>> PlayerSpinnedEndedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C>> PlayerStartedExposedToLavaNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C>> PlayerEndedExposedToLavaNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> PowerUpBallSpawnedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> PowerUpBallObtainedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> PlayerSwitchTeamNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<StartMatchCountdownNetEventS2C>> StartMatchCountdownNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<StopMatchCountdownNetEventS2C>> StopMatchCountdownNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>> StartMatchEligibleChangedNetEventsPerClient { get; }
        public CapacityDict<long, FixedClassUnorderedList<StageEndNetEventS2C>> StageEndNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<TeamLostNetEventS2C>> TeamLostNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<TalentSwitchNetEventS2C>> TalentSwitchNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<GainBoltsNetEventS2C>> GainBoltsNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>> EnvironmentSpringPlayerCollisionNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C>> EnvironmentSpikePlayerCollisionNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>> PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PreparationPhaseEndedNetEventS2C>> PreparationPhaseEndedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<CreateSwapFieldNetEventS2C>> CreateSwapFieldNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<CreateKOProjectileNetEventS2C>> CreateKOProjectileNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> KOProjectHitPlayerNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateKOTalentNetEventS2C>> DeactivateKOTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PerformDashPulseNetEventS2C>> PerformDashPulseNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>> ActivateSentryGunTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>> DeactivateSentryGunTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>> UpdatePlayerTalentStocksNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>> PlayerMaxShootCooldownChangedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>> CreateGrapplingHookProjectileNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<GrapplingHookHitWallNetEventS2C>> GrapplingHookHitWallNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>> DeactivateGrapplingHookTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<CreateFishingRodProjectileNetEventS2C>> CreateFishingRodProjectileNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C>> FishingRodCaughtEnemyNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<FishingRodTipHitWallNetEventS2C>> FishingRodTipHitWallNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<FishingRodThrowNetEventS2C>> FishingRodThrowNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C>> DeactivateFishingRodTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<CreateSoulGhostNetEventS2C>> CreateSoulGhostNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateSoulTalentNetEventS2C>> DeactivateSoulTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateRockTalentNetEventS2C>> ActivateRockTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateRockTalentNetEventS2C>> DeactivateRockTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateFrozenTalentNetEventS2C>> ActivateFrozenTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateFrozenTalentNetEventS2C>> DeactivateFrozenTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<MoleSpawnedNetEventS2C>> MoleSpawnedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<MoleHitNetEventS2C>> MoleHitNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<MoleExpiredNetEventS2C>> MoleExpiredNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<GoldenMoleDamagedNetEventS2C>> GoldenMoleDamagedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ScoreGatePassedNetEventS2C>> ScoreGatePassedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<GateTrapClosingNetEventS2C>> GateTrapClosingNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>> CreateMagneticPullFieldNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>> ActivateUmbrellaTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>> DeactivateUmbrellaTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateWaterGunTalentNetEventS2C>> ActivateWaterGunTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C>> DeactivateWaterGunTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C>> ActivateHeadbuttChargingNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PerformHeadbuttDashNetEventS2C>> PerformHeadbuttDashNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PerformBarrelDashNetEventS2C>> PerformBarrelDashNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<HeadbuttHitEnemyNetEventS2C>> HeadbuttHitEnemyNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C>> DeactivateHeadbuttTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<LayChickenEggNetEventS2C>> LayChickenEggNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ChickenEggHitNetEventS2C>> ChickenEggHitNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>> ActivateYearsOfPainTalentNetEventsPerClient { get; }
        public CapacityDict<long, FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>> PlayerLockOnTargetsChangedNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>> PlayerLockedOnTargetHitNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PlayerPowerUpChangedNetEventS2C>> PlayerPowerUpChangedNetEventsPerClient { get; }
        public CapacityDict<long, FixedClassUnorderedList<ActivateSonicSlapNetEventS2C>> ActivateSonicSlapNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<PerformGalacticPullNetEventS2C>> PerformGalacticPullNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C>> DeactivateGalacticForceFieldNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateNukePowerUpNetEventS2C>> ActivateNukePowerUpNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C>> DeactivateShufflePowerUpNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C>> ShuffleSwapPlayerPositionNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ActivateShuffleNetEventS2C>> ActivateShuffleNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C>> StartPowerUpGrantingPhaseNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C>> EndPowerUpGrantingPhaseNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<ShootFrigidBlockNetEventS2C>> ShootFrigidBlockNetEventsPerClient { get; }
        public CapacityDict<long, FixedUnorderedList<DestroyFrigidBlockNetEventS2C>> DestroyFrigidBlockNetEventsPerClient { get; }

        private readonly ConcurrentPool<FixedUnorderedList<BulletSpawnNetEventS2C>> _bulletSpawnListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>> _playerRejoinAcceptListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>> _matchMakingPlayerJoinAcceptListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerTakeDamageNetEventS2C>> _playerTakeDamageListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerDiedNetEventS2C>> _playerDiedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<BulletDestroyedNetEventS2C>> _bulletDestroyedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayersSwapNetEventS2C>> _playerSwapListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<TalentCardObtainedNetEventS2C>> _talentCardObtainedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<TalentCardHitNetEventS2C>> _talentCardHitListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>> _playerSpinnedStartedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>> _playerSpinnedEndedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C>> _playerStartedExposedToLavaListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C>> _playerEndedExposedToLavaListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>> _powerUpBallsSpawnedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PowerUpBallObtainedNetEventS2C>> _powerUpBallsObtainedListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerSwitchTeamNetEventS2C>> _playerSwitchTeamListPool;
        private readonly ConcurrentPool<FixedUnorderedList<StartMatchCountdownNetEventS2C>> _startMatchCountdownListPool;
        private readonly ConcurrentPool<FixedUnorderedList<StopMatchCountdownNetEventS2C>> _stopMatchCountdownListPool;
        private readonly ConcurrentPool<FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>> _startMatchEligibleChangedListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<StageEndNetEventS2C>> _stageEndNetEventsListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>> _playerLockOnTargetsChangedNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>> _playerLockOnTargetHitNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PlayerPowerUpChangedNetEventS2C>> _playerPowerUpChangedNetEventsListPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<ActivateSonicSlapNetEventS2C>> _sonicSlapActivatedNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<TeamLostNetEventS2C>> _teamLostNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<TalentSwitchNetEventS2C>> _talentSwitchNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>> _environmentSpringPlayerCollisionListPool;
        private readonly ConcurrentPool<FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C>> _environmentSpikePlayerCollisionListPool;
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
        private readonly ConcurrentPool<FixedUnorderedList<CreateFishingRodProjectileNetEventS2C>> _createFishingRodProjectileNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C>> _fishingRodCaughtEnemyNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<FishingRodTipHitWallNetEventS2C>> _fishingRodTipHitWallNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<FishingRodThrowNetEventS2C>> _fishingRodThrowNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C>> _deactivateFishingRodTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<CreateSoulGhostNetEventS2C>> _createSoulGhostNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateSoulTalentNetEventS2C>> _deactivateSoulTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateRockTalentNetEventS2C>> _activateRockTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateRockTalentNetEventS2C>> _deactivateRockTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateFrozenTalentNetEventS2C>> _activateFrozenTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateFrozenTalentNetEventS2C>> _deactivateFrozenTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<MoleSpawnedNetEventS2C>> _moleSpawnedNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<MoleHitNetEventS2C>> _moleHitNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<MoleExpiredNetEventS2C>> _moleExpiredNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<GoldenMoleDamagedNetEventS2C>> _goldenMoleDamagedNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ScoreGatePassedNetEventS2C>> _scoreGatePassedNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<GateTrapClosingNetEventS2C>> _gateTrapClosingNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>> _createMagneticPullFieldNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>> _activateUmbrellaTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>> _deactivateUmbrellaTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateWaterGunTalentNetEventS2C>> _activateWaterGunTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C>> _deactivateWaterGunTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C>> _activateHeadbuttChargingNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PerformHeadbuttDashNetEventS2C>> _performHeadbuttDashNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PerformBarrelDashNetEventS2C>> _performBarrelDashNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<HeadbuttHitEnemyNetEventS2C>> _headbuttHitEnemyNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C>> _deactivateHeadbuttTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<LayChickenEggNetEventS2C>> _layChickenEggNetEventsPool;
        private readonly ConcurrentPool<FixedUnorderedList<ChickenEggHitNetEventS2C>> _chickenEggHitNetEventsPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>> _activateYearsOfPainTalentNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<PerformGalacticPullNetEventS2C>> _performGalacticPullNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C>> _deactivateGalacticForceFieldNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateNukePowerUpNetEventS2C>> _activateNukePowerUpNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C>> _activateShufflePowerUpNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C>> _shuffleSwapPlayerPositionNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ActivateShuffleNetEventS2C>> _activateShuffleNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C>> _startPowerUpGrantingPhaseNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C>> _endPowerUpGrantingPhaseNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<ShootFrigidBlockNetEventS2C>> _shootFrigidBlockNetEventsListPool;
        private readonly ConcurrentPool<FixedUnorderedList<DestroyFrigidBlockNetEventS2C>> _destroyFrigidBlockNetEventsListPool;

        public NetEventsDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            var maxConcurrentPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            BulletSpawnNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<BulletSpawnNetEventS2C>>(maxConcurrentPlayers);
            PlayerRejoinAcceptNetEventsPerClient = new CapacityDict<long, FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>>(maxConcurrentPlayers);
            MatchMakingPlayerJoinAcceptNetEventsPerClient = new CapacityDict<long, FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>>(maxConcurrentPlayers);
            PlayerTakeDamageNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerTakeDamageNetEventS2C>>(maxConcurrentPlayers);
            PlayerDiedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerDiedNetEventS2C>>(maxConcurrentPlayers);
            BulletDestroyedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<BulletDestroyedNetEventS2C>>(maxConcurrentPlayers);
            PlayerSwapNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayersSwapNetEventS2C>>(maxConcurrentPlayers);
            TalentCardObtainedNetEventsPerClient = new CapacityDict<long, FixedClassUnorderedList<TalentCardObtainedNetEventS2C>>(maxConcurrentPlayers);
            TalentCardHitNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<TalentCardHitNetEventS2C>>(maxConcurrentPlayers);
            PlayerSpinnedStartedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>>(maxConcurrentPlayers);
            PlayerSpinnedEndedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>>(maxConcurrentPlayers);
            PlayerStartedExposedToLavaNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C>>(maxConcurrentPlayers);
            PlayerEndedExposedToLavaNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C>>(maxConcurrentPlayers);
            PowerUpBallSpawnedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>>(maxConcurrentPlayers);
            PowerUpBallObtainedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PowerUpBallObtainedNetEventS2C>>(maxConcurrentPlayers);
            PlayerSwitchTeamNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerSwitchTeamNetEventS2C>>(maxConcurrentPlayers);
            StartMatchCountdownNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<StartMatchCountdownNetEventS2C>>(maxConcurrentPlayers);
            StopMatchCountdownNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<StopMatchCountdownNetEventS2C>>(maxConcurrentPlayers);
            StartMatchEligibleChangedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>>(maxConcurrentPlayers);
            StageEndNetEventsPerClient = new CapacityDict<long, FixedClassUnorderedList<StageEndNetEventS2C>>(maxConcurrentPlayers);
            PlayerLockOnTargetsChangedNetEventsPerClient = new CapacityDict<long, FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>>(maxConcurrentPlayers);
            PlayerLockedOnTargetHitNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>>(maxConcurrentPlayers);
            PlayerPowerUpChangedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerPowerUpChangedNetEventS2C>>(maxConcurrentPlayers);
            ActivateSonicSlapNetEventsPerClient = new CapacityDict<long, FixedClassUnorderedList<ActivateSonicSlapNetEventS2C>>(maxConcurrentPlayers);
            TeamLostNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<TeamLostNetEventS2C>>(maxConcurrentPlayers);
            TalentSwitchNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<TalentSwitchNetEventS2C>>(maxConcurrentPlayers);
            EnvironmentSpringPlayerCollisionNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>>(maxConcurrentPlayers);
            EnvironmentSpikePlayerCollisionNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C>>(maxConcurrentPlayers);
            GainBoltsNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<GainBoltsNetEventS2C>>(maxConcurrentPlayers);
            PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>>(maxConcurrentPlayers);
            PreparationPhaseEndedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PreparationPhaseEndedNetEventS2C>>(maxConcurrentPlayers);
            CreateSwapFieldNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<CreateSwapFieldNetEventS2C>>(maxConcurrentPlayers);
            DeactivateSwapTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(maxConcurrentPlayers);
            CreateKOProjectileNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<CreateKOProjectileNetEventS2C>>(maxConcurrentPlayers);
            KOProjectHitPlayerNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(maxConcurrentPlayers);
            DeactivateKOTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(maxConcurrentPlayers);
            ActivateSentryGunTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>>(maxConcurrentPlayers);
            DeactivateSentryGunTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>>(maxConcurrentPlayers);
            PerformDashPulseNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PerformDashPulseNetEventS2C>>(maxConcurrentPlayers);
            UpdatePlayerTalentStocksNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>>(maxConcurrentPlayers);
            PlayerMaxShootCooldownChangedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>>(maxConcurrentPlayers);
            CreateGrapplingHookProjectileNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>>(maxConcurrentPlayers);
            GrapplingHookHitWallNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<GrapplingHookHitWallNetEventS2C>>(maxConcurrentPlayers);
            DeactivateGrapplingHookTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>>(maxConcurrentPlayers);
            CreateFishingRodProjectileNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<CreateFishingRodProjectileNetEventS2C>>(maxConcurrentPlayers);
            FishingRodCaughtEnemyNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C>>(maxConcurrentPlayers);
            FishingRodTipHitWallNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<FishingRodTipHitWallNetEventS2C>>(maxConcurrentPlayers);
            FishingRodThrowNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<FishingRodThrowNetEventS2C>>(maxConcurrentPlayers);
            DeactivateFishingRodTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C>>(maxConcurrentPlayers);
            CreateSoulGhostNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<CreateSoulGhostNetEventS2C>>(maxConcurrentPlayers);
            DeactivateSoulTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateSoulTalentNetEventS2C>>(maxConcurrentPlayers);
            ActivateRockTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateRockTalentNetEventS2C>>(maxConcurrentPlayers);
            DeactivateRockTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateRockTalentNetEventS2C>>(maxConcurrentPlayers);
            ActivateFrozenTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateFrozenTalentNetEventS2C>>(maxConcurrentPlayers);
            DeactivateFrozenTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateFrozenTalentNetEventS2C>>(maxConcurrentPlayers);
            MoleSpawnedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<MoleSpawnedNetEventS2C>>(maxConcurrentPlayers);
            MoleHitNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<MoleHitNetEventS2C>>(maxConcurrentPlayers);
            MoleExpiredNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<MoleExpiredNetEventS2C>>(maxConcurrentPlayers);
            GoldenMoleDamagedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<GoldenMoleDamagedNetEventS2C>>(maxConcurrentPlayers);
            ScoreGatePassedNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ScoreGatePassedNetEventS2C>>(maxConcurrentPlayers);
            GateTrapClosingNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<GateTrapClosingNetEventS2C>>(maxConcurrentPlayers);
            CreateMagneticPullFieldNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>>(maxConcurrentPlayers);
            ActivateUmbrellaTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>>(maxConcurrentPlayers);
            DeactivateUmbrellaTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>>(maxConcurrentPlayers);
            ActivateWaterGunTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateWaterGunTalentNetEventS2C>>(maxConcurrentPlayers);
            DeactivateWaterGunTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C>>(maxConcurrentPlayers);
            ActivateHeadbuttChargingNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C>>(maxConcurrentPlayers);
            PerformHeadbuttDashNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PerformHeadbuttDashNetEventS2C>>(maxConcurrentPlayers);
            PerformBarrelDashNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PerformBarrelDashNetEventS2C>>(maxConcurrentPlayers);
            HeadbuttHitEnemyNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<HeadbuttHitEnemyNetEventS2C>>(maxConcurrentPlayers);
            DeactivateHeadbuttTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C>>(maxConcurrentPlayers);
            LayChickenEggNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<LayChickenEggNetEventS2C>>(maxConcurrentPlayers);
            ChickenEggHitNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ChickenEggHitNetEventS2C>>(maxConcurrentPlayers);
            ActivateYearsOfPainTalentNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>>(maxConcurrentPlayers);
            PerformGalacticPullNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<PerformGalacticPullNetEventS2C>>(maxConcurrentPlayers);
            DeactivateGalacticForceFieldNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C>>(maxConcurrentPlayers);
            ActivateNukePowerUpNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateNukePowerUpNetEventS2C>>(maxConcurrentPlayers);
            DeactivateShufflePowerUpNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C>>(maxConcurrentPlayers);
            ShuffleSwapPlayerPositionNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C>>(maxConcurrentPlayers);
            ActivateShuffleNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ActivateShuffleNetEventS2C>>(maxConcurrentPlayers);
            StartPowerUpGrantingPhaseNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C>>(maxConcurrentPlayers);
            EndPowerUpGrantingPhaseNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C>>(maxConcurrentPlayers);
            ShootFrigidBlockNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<ShootFrigidBlockNetEventS2C>>(maxConcurrentPlayers);
            DestroyFrigidBlockNetEventsPerClient = new CapacityDict<long, FixedUnorderedList<DestroyFrigidBlockNetEventS2C>>(maxConcurrentPlayers);
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
            _playerSpinnedStartedListPool = new ConcurrentPool<FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>>(() => new FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>(networkConfig.MaxCap.PlayerSpinnedStartedNetEvents), maxConcurrentPlayers);
            _playerSpinnedEndedListPool = new ConcurrentPool<FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>>(() => new FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>(networkConfig.MaxCap.PlayerSpinnedEndedNetEvents), maxConcurrentPlayers);
            _playerStartedExposedToLavaListPool = new ConcurrentPool<FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C>>(() => new FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C>(networkConfig.MaxCap.PlayerStartedExposedToLavaNetEvents), maxConcurrentPlayers);
            _playerEndedExposedToLavaListPool = new ConcurrentPool<FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C>>(() => new FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C>(networkConfig.MaxCap.PlayerEndedExposedToLavaNetEvents), maxConcurrentPlayers);
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
            _playerLockOnTargetsChangedNetEventsListPool = new ConcurrentPool<FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>>(() =>
            {
                var list = new FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>(networkConfig.MaxCap.PlayerLockOnTargetsChangedNetEvents, () => new PlayerLockOnTargetsChangedNetEventS2C(networkConfig.MaxCap.ConcurrentLockOnTargets));
                list.Clear();
                return list;
            }, maxConcurrentPlayers);

            _playerLockOnTargetHitNetEventsListPool = new ConcurrentPool<FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>>(() => new FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>(networkConfig.MaxCap.PlayerLockOnTargetHitNetEvents), maxConcurrentPlayers);
            _playerPowerUpChangedNetEventsListPool = new ConcurrentPool<FixedUnorderedList<PlayerPowerUpChangedNetEventS2C>>(() => new FixedUnorderedList<PlayerPowerUpChangedNetEventS2C>(networkConfig.MaxCap.PlayerPowerUpChangedNetEvents), maxConcurrentPlayers);
            _sonicSlapActivatedNetEventsListPool = new ConcurrentPool<FixedClassUnorderedList<ActivateSonicSlapNetEventS2C>>(() =>
            {
                var list = new FixedClassUnorderedList<ActivateSonicSlapNetEventS2C>(networkConfig.MaxCap.ActivateSonicSlapNetEvents, () => new ActivateSonicSlapNetEventS2C(networkConfig.MaxCap.ConcurrentEnemyPlayers));
                list.Clear();
                return list;
            }, maxConcurrentPlayers);
            _teamLostNetEventsListPool = new ConcurrentPool<FixedUnorderedList<TeamLostNetEventS2C>>(() => new FixedUnorderedList<TeamLostNetEventS2C>(sharedGamePlayConfig.MaxTeamsAmount), maxConcurrentPlayers);
            _talentSwitchNetEventsListPool = new ConcurrentPool<FixedUnorderedList<TalentSwitchNetEventS2C>>(() => new FixedUnorderedList<TalentSwitchNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);
            _environmentSpringPlayerCollisionListPool = new ConcurrentPool<FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>>(() => new FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>(networkConfig.MaxCap.EnvironmentSpringPlayerCollisionNetEvents), maxConcurrentPlayers);
            _environmentSpikePlayerCollisionListPool = new ConcurrentPool<FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C>>(() => new FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C>(networkConfig.MaxCap.EnvironmentSpikePlayerCollisionNetEvents), maxConcurrentPlayers);
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
            _createFishingRodProjectileNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateFishingRodProjectileNetEventS2C>>(() => new FixedUnorderedList<CreateFishingRodProjectileNetEventS2C>(networkConfig.MaxCap.CreateFishingRodProjectileNetEvents), maxConcurrentPlayers);
            _fishingRodCaughtEnemyNetEventsListPool = new ConcurrentPool<FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C>>(() => new FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C>(networkConfig.MaxCap.FishingRodCaughtEnemyNetEvents), maxConcurrentPlayers);
            _fishingRodTipHitWallNetEventsListPool = new ConcurrentPool<FixedUnorderedList<FishingRodTipHitWallNetEventS2C>>(() => new FixedUnorderedList<FishingRodTipHitWallNetEventS2C>(networkConfig.MaxCap.FishingRodTipHitWallNetEvents), maxConcurrentPlayers);
            _fishingRodThrowNetEventsListPool = new ConcurrentPool<FixedUnorderedList<FishingRodThrowNetEventS2C>>(() => new FixedUnorderedList<FishingRodThrowNetEventS2C>(networkConfig.MaxCap.FishingRodThrowNetEvents), maxConcurrentPlayers);
            _deactivateFishingRodTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C>(networkConfig.MaxCap.DeactivateFishingRodTalentNetEvents), maxConcurrentPlayers);
            _createSoulGhostNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateSoulGhostNetEventS2C>>(() => new FixedUnorderedList<CreateSoulGhostNetEventS2C>(networkConfig.MaxCap.CreateSoulGhostNetEvents), maxConcurrentPlayers);
            _deactivateSoulTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateSoulTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateSoulTalentNetEventS2C>(networkConfig.MaxCap.DeactivateSoulTalentNetEvents), maxConcurrentPlayers);
            _activateRockTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateRockTalentNetEventS2C>>(() => new FixedUnorderedList<ActivateRockTalentNetEventS2C>(networkConfig.MaxCap.ActivateRockTalentNetEvents), maxConcurrentPlayers);
            _deactivateRockTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateRockTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateRockTalentNetEventS2C>(networkConfig.MaxCap.DeactivateRockTalentNetEvents), maxConcurrentPlayers);
            _activateFrozenTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateFrozenTalentNetEventS2C>>(() => new FixedUnorderedList<ActivateFrozenTalentNetEventS2C>(networkConfig.MaxCap.ActivateFrozenTalentNetEvents), maxConcurrentPlayers);
            _deactivateFrozenTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateFrozenTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateFrozenTalentNetEventS2C>(networkConfig.MaxCap.DeactivateFrozenTalentNetEvents), maxConcurrentPlayers);
            _moleSpawnedNetEventsListPool = new ConcurrentPool<FixedUnorderedList<MoleSpawnedNetEventS2C>>(() => new FixedUnorderedList<MoleSpawnedNetEventS2C>(networkConfig.MaxCap.MoleSpawnedNetEvents), maxConcurrentPlayers);
            _moleHitNetEventsListPool = new ConcurrentPool<FixedUnorderedList<MoleHitNetEventS2C>>(() => new FixedUnorderedList<MoleHitNetEventS2C>(networkConfig.MaxCap.MoleHitNetEvents), maxConcurrentPlayers);
            _moleExpiredNetEventsListPool = new ConcurrentPool<FixedUnorderedList<MoleExpiredNetEventS2C>>(() => new FixedUnorderedList<MoleExpiredNetEventS2C>(networkConfig.MaxCap.MoleExpiredNetEvents), maxConcurrentPlayers);
            _goldenMoleDamagedNetEventsListPool = new ConcurrentPool<FixedUnorderedList<GoldenMoleDamagedNetEventS2C>>(() => new FixedUnorderedList<GoldenMoleDamagedNetEventS2C>(networkConfig.MaxCap.GoldenMoleDamagedNetEvents), maxConcurrentPlayers);
            _scoreGatePassedNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ScoreGatePassedNetEventS2C>>(() => new FixedUnorderedList<ScoreGatePassedNetEventS2C>(networkConfig.MaxCap.ScoreGatePassedNetEvents), maxConcurrentPlayers);
            _gateTrapClosingNetEventsListPool = new ConcurrentPool<FixedUnorderedList<GateTrapClosingNetEventS2C>>(() => new FixedUnorderedList<GateTrapClosingNetEventS2C>(networkConfig.MaxCap.GateTrapClosingNetEvents), maxConcurrentPlayers);
            _createMagneticPullFieldNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>>(() => new FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>(networkConfig.MaxCap.CreateMagneticPullFieldNetEvents), maxConcurrentPlayers);
            _activateUmbrellaTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>>(() => new FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>(networkConfig.MaxCap.ActivateUmbrellaTalentNetEvents), maxConcurrentPlayers);
            _deactivateUmbrellaTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>(networkConfig.MaxCap.DeactivateUmbrellaTalentNetEvents), maxConcurrentPlayers);
            _activateWaterGunTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateWaterGunTalentNetEventS2C>>(() => new FixedUnorderedList<ActivateWaterGunTalentNetEventS2C>(networkConfig.MaxCap.ActivateWaterGunTalentNetEvents), maxConcurrentPlayers);
            _deactivateWaterGunTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C>(networkConfig.MaxCap.DeactivateWaterGunTalentNetEvents), maxConcurrentPlayers);
            _activateHeadbuttChargingNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C>>(() => new FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C>(networkConfig.MaxCap.ActivateHeadbuttChargingNetEvents), maxConcurrentPlayers);
            _performHeadbuttDashNetEventsListPool = new ConcurrentPool<FixedUnorderedList<PerformHeadbuttDashNetEventS2C>>(() => new FixedUnorderedList<PerformHeadbuttDashNetEventS2C>(networkConfig.MaxCap.PerformHeadbuttDashNetEvents), maxConcurrentPlayers);
            _performBarrelDashNetEventsListPool = new ConcurrentPool<FixedUnorderedList<PerformBarrelDashNetEventS2C>>(() => new FixedUnorderedList<PerformBarrelDashNetEventS2C>(networkConfig.MaxCap.PerformBarrelDashNetEvents), maxConcurrentPlayers);
            _headbuttHitEnemyNetEventsListPool = new ConcurrentPool<FixedUnorderedList<HeadbuttHitEnemyNetEventS2C>>(() => new FixedUnorderedList<HeadbuttHitEnemyNetEventS2C>(networkConfig.MaxCap.HeadbuttHitEnemyNetEvents), maxConcurrentPlayers);
            _deactivateHeadbuttTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C>(networkConfig.MaxCap.DeactivateHeadbuttTalentNetEvents), maxConcurrentPlayers);
            _layChickenEggNetEventsPool = new ConcurrentPool<FixedUnorderedList<LayChickenEggNetEventS2C>>(() => new FixedUnorderedList<LayChickenEggNetEventS2C>(networkConfig.MaxCap.LayChickenEggNetEvents), maxConcurrentPlayers);
            _chickenEggHitNetEventsPool = new ConcurrentPool<FixedUnorderedList<ChickenEggHitNetEventS2C>>(() => new FixedUnorderedList<ChickenEggHitNetEventS2C>(networkConfig.MaxCap.ChickenEggHitNetEvents), maxConcurrentPlayers);
            _activateYearsOfPainTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>>(() => new FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>(networkConfig.MaxCap.ActivateYearsOfPainTalentNetEvents), maxConcurrentPlayers);
            _performGalacticPullNetEventsListPool = new ConcurrentPool<FixedUnorderedList<PerformGalacticPullNetEventS2C>>(() => new FixedUnorderedList<PerformGalacticPullNetEventS2C>(networkConfig.MaxCap.PerformGalacticPullNetEvents), maxConcurrentPlayers);
            _deactivateGalacticForceFieldNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C>>(() => new FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C>(networkConfig.MaxCap.DeactivateGalacticForceFieldNetEvents), maxConcurrentPlayers);
            _activateNukePowerUpNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateNukePowerUpNetEventS2C>>(() => new FixedUnorderedList<ActivateNukePowerUpNetEventS2C>(networkConfig.MaxCap.ActivateNukePowerUpNetEvents), maxConcurrentPlayers);
            _activateShufflePowerUpNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C>>(() => new FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C>(networkConfig.MaxCap.ActivateShufflePowerUpNetEvents), maxConcurrentPlayers);
            _shuffleSwapPlayerPositionNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C>>(() => new FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C>(networkConfig.MaxCap.ShuffleSwapPlayerPositionNetEvents), maxConcurrentPlayers);
            _activateShuffleNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ActivateShuffleNetEventS2C>>(() => new FixedUnorderedList<ActivateShuffleNetEventS2C>(networkConfig.MaxCap.ActivateShufflePowerUpNetEvents), maxConcurrentPlayers);
            _startPowerUpGrantingPhaseNetEventsListPool = new ConcurrentPool<FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C>>(() => new FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C>(networkConfig.MaxCap.StartPowerUpGrantingPhaseNetEvents), maxConcurrentPlayers);
            _endPowerUpGrantingPhaseNetEventsListPool = new ConcurrentPool<FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C>>(() => new FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C>(networkConfig.MaxCap.EndPowerUpGrantingPhaseNetEvents), maxConcurrentPlayers);
            _shootFrigidBlockNetEventsListPool = new ConcurrentPool<FixedUnorderedList<ShootFrigidBlockNetEventS2C>>(() => new FixedUnorderedList<ShootFrigidBlockNetEventS2C>(networkConfig.MaxCap.ShootFrigidBlockNetEvents), maxConcurrentPlayers);
            _destroyFrigidBlockNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DestroyFrigidBlockNetEventS2C>>(() => new FixedUnorderedList<DestroyFrigidBlockNetEventS2C>(networkConfig.MaxCap.DestroyFrigidBlockNetEvents), maxConcurrentPlayers);
        }

        public void StartSavingClientEvents(long clientId)
        {
            if (!BulletSpawnNetEventsPerClient.ContainsKey(clientId)) // don't use TryAdd since it will _bulletSpawnListPool.Get() an object from the pool! 
            {
                BulletSpawnNetEventsPerClient.Add(clientId, _bulletSpawnListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!PlayerRejoinAcceptNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerRejoinAcceptNetEventsPerClient.Add(clientId, _playerRejoinAcceptListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }
            
            if (!MatchMakingPlayerJoinAcceptNetEventsPerClient.ContainsKey(clientId))
            {
                MatchMakingPlayerJoinAcceptNetEventsPerClient.Add(clientId, _matchMakingPlayerJoinAcceptListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }
            
            if (!PlayerTakeDamageNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerTakeDamageNetEventsPerClient.Add(clientId, _playerTakeDamageListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!PlayerDiedNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerDiedNetEventsPerClient.Add(clientId, _playerDiedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }
            
            if (!BulletDestroyedNetEventsPerClient.ContainsKey(clientId))
            {
                BulletDestroyedNetEventsPerClient.Add(clientId, _bulletDestroyedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }    
            
            if (!PlayerSwapNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerSwapNetEventsPerClient.Add(clientId, _playerSwapListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!TalentCardObtainedNetEventsPerClient.ContainsKey(clientId))
            {
                TalentCardObtainedNetEventsPerClient.Add(clientId, _talentCardObtainedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }
            
            if (!TalentCardHitNetEventsPerClient.ContainsKey(clientId))
            {
                TalentCardHitNetEventsPerClient.Add(clientId, _talentCardHitListPool.Get());
            }

            if (!PlayerSpinnedStartedNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerSpinnedStartedNetEventsPerClient.Add(clientId, _playerSpinnedStartedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!PlayerStartedExposedToLavaNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerStartedExposedToLavaNetEventsPerClient.Add(clientId, _playerStartedExposedToLavaListPool.Get());
            }

            if (!PlayerEndedExposedToLavaNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerEndedExposedToLavaNetEventsPerClient.Add(clientId, _playerEndedExposedToLavaListPool.Get());
            }

            if (!PlayerSpinnedEndedNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerSpinnedEndedNetEventsPerClient.Add(clientId, _playerSpinnedEndedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }
            
            if (!PowerUpBallSpawnedNetEventsPerClient.ContainsKey(clientId))
            {
                PowerUpBallSpawnedNetEventsPerClient.Add(clientId, _powerUpBallsSpawnedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }
            
            if (!PowerUpBallObtainedNetEventsPerClient.ContainsKey(clientId))
            {
                PowerUpBallObtainedNetEventsPerClient.Add(clientId, _powerUpBallsObtainedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!PlayerSwitchTeamNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerSwitchTeamNetEventsPerClient.Add(clientId, _playerSwitchTeamListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!StartMatchCountdownNetEventsPerClient.ContainsKey(clientId))
            {
                StartMatchCountdownNetEventsPerClient.Add(clientId, _startMatchCountdownListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!StopMatchCountdownNetEventsPerClient.ContainsKey(clientId))
            {
                StopMatchCountdownNetEventsPerClient.Add(clientId, _stopMatchCountdownListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!StageEndNetEventsPerClient.ContainsKey(clientId))
            {
                StageEndNetEventsPerClient.Add(clientId, _stageEndNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            
            if (!PlayerLockOnTargetsChangedNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerLockOnTargetsChangedNetEventsPerClient.Add(clientId, _playerLockOnTargetsChangedNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }
            
            if (!PlayerLockedOnTargetHitNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerLockedOnTargetHitNetEventsPerClient.Add(clientId, _playerLockOnTargetHitNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!PlayerPowerUpChangedNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerPowerUpChangedNetEventsPerClient.Add(clientId, _playerPowerUpChangedNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!ActivateSonicSlapNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateSonicSlapNetEventsPerClient.Add(clientId, _sonicSlapActivatedNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!TeamLostNetEventsPerClient.ContainsKey(clientId))
            {
                TeamLostNetEventsPerClient.Add(clientId, _teamLostNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!TalentSwitchNetEventsPerClient.ContainsKey(clientId))
            {
                TalentSwitchNetEventsPerClient.Add(clientId, _talentSwitchNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!StartMatchEligibleChangedNetEventsPerClient.ContainsKey(clientId))
            {
                StartMatchEligibleChangedNetEventsPerClient.Add(clientId, _startMatchEligibleChangedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!EnvironmentSpringPlayerCollisionNetEventsPerClient.ContainsKey(clientId))
            {
                EnvironmentSpringPlayerCollisionNetEventsPerClient.Add(clientId, _environmentSpringPlayerCollisionListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!EnvironmentSpikePlayerCollisionNetEventsPerClient.ContainsKey(clientId))
            {
                EnvironmentSpikePlayerCollisionNetEventsPerClient.Add(clientId, _environmentSpikePlayerCollisionListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!GainBoltsNetEventsPerClient.ContainsKey(clientId))
            {
                GainBoltsNetEventsPerClient.Add(clientId, _gainBoltsNetEventsListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient.Add(clientId, _playerToEnvironmentTeleportGateCollisionListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!PreparationPhaseEndedNetEventsPerClient.ContainsKey(clientId))
            {
                PreparationPhaseEndedNetEventsPerClient.Add(clientId, _preparationPhaseEndedListPool.Get());
            }
            else
            {
                LogService.LogError($"Player already exists! {clientId}");
            }

            if (!CreateSwapFieldNetEventsPerClient.ContainsKey(clientId))
            {
                CreateSwapFieldNetEventsPerClient.Add(clientId, _createSwapFieldNetEventsListPool.Get());
            }
            if (!DeactivateSwapTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateSwapTalentNetEventsPerClient.Add(clientId, _deactivateSwapTalentNetEventsListPool.Get());
            }
            if (!CreateKOProjectileNetEventsPerClient.ContainsKey(clientId))
            {
                CreateKOProjectileNetEventsPerClient.Add(clientId, _createKOProjectileNetEventsListPool.Get());
            }
            if (!KOProjectHitPlayerNetEventsPerClient.ContainsKey(clientId))
            {
                KOProjectHitPlayerNetEventsPerClient.Add(clientId, _koProjectHitPlayerNetEventsListPool.Get());
            }
            if (!DeactivateKOTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateKOTalentNetEventsPerClient.Add(clientId, _deactivateKOTalentNetEventsListPool.Get());
            }
            if (!PerformDashPulseNetEventsPerClient.ContainsKey(clientId))
            {
                PerformDashPulseNetEventsPerClient.Add(clientId, _performDashPulseNetEventsListPool.Get());
            }
            if (!ActivateSentryGunTalentNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateSentryGunTalentNetEventsPerClient.Add(clientId, _activateSentryGunTalentNetEventsListPool.Get());
            }
            if (!DeactivateSentryGunTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateSentryGunTalentNetEventsPerClient.Add(clientId, _deactivateSentryGunTalentNetEventsListPool.Get());
            }
            if (!UpdatePlayerTalentStocksNetEventsPerClient.ContainsKey(clientId))
            {
                UpdatePlayerTalentStocksNetEventsPerClient.Add(clientId, _updatePlayerTalentStocksNetEventsListPool.Get());
            }
            if (!PlayerMaxShootCooldownChangedNetEventsPerClient.ContainsKey(clientId))
            {
                PlayerMaxShootCooldownChangedNetEventsPerClient.Add(clientId, _playerMaxShootCooldownChangedListPool.Get());
            }
            if (!CreateGrapplingHookProjectileNetEventsPerClient.ContainsKey(clientId))
            {
                CreateGrapplingHookProjectileNetEventsPerClient.Add(clientId, _createGrapplingHookProjectileNetEventsListPool.Get());
            }
            if (!GrapplingHookHitWallNetEventsPerClient.ContainsKey(clientId))
            {
                GrapplingHookHitWallNetEventsPerClient.Add(clientId, _grapplingHookHitWallNetEventsListPool.Get());
            }
            if (!DeactivateGrapplingHookTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateGrapplingHookTalentNetEventsPerClient.Add(clientId, _deactivateGrapplingHookTalentNetEventsListPool.Get());
            }
            if (!CreateFishingRodProjectileNetEventsPerClient.ContainsKey(clientId))
            {
                CreateFishingRodProjectileNetEventsPerClient.Add(clientId, _createFishingRodProjectileNetEventsListPool.Get());
            }
            if (!FishingRodCaughtEnemyNetEventsPerClient.ContainsKey(clientId))
            {
                FishingRodCaughtEnemyNetEventsPerClient.Add(clientId, _fishingRodCaughtEnemyNetEventsListPool.Get());
            }
            if (!FishingRodTipHitWallNetEventsPerClient.ContainsKey(clientId))
            {
                FishingRodTipHitWallNetEventsPerClient.Add(clientId, _fishingRodTipHitWallNetEventsListPool.Get());
            }
            if (!FishingRodThrowNetEventsPerClient.ContainsKey(clientId))
            {
                FishingRodThrowNetEventsPerClient.Add(clientId, _fishingRodThrowNetEventsListPool.Get());
            }
            if (!DeactivateFishingRodTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateFishingRodTalentNetEventsPerClient.Add(clientId, _deactivateFishingRodTalentNetEventsListPool.Get());
            }
            if (!CreateSoulGhostNetEventsPerClient.ContainsKey(clientId))
            {
                CreateSoulGhostNetEventsPerClient.Add(clientId, _createSoulGhostNetEventsListPool.Get());
            }
            if (!DeactivateSoulTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateSoulTalentNetEventsPerClient.Add(clientId, _deactivateSoulTalentNetEventsListPool.Get());
            }
            if (!ActivateRockTalentNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateRockTalentNetEventsPerClient.Add(clientId, _activateRockTalentNetEventsListPool.Get());
            }
            if (!DeactivateRockTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateRockTalentNetEventsPerClient.Add(clientId, _deactivateRockTalentNetEventsListPool.Get());
            }
            if (!ActivateFrozenTalentNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateFrozenTalentNetEventsPerClient.Add(clientId, _activateFrozenTalentNetEventsListPool.Get());
            }
            if (!DeactivateFrozenTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateFrozenTalentNetEventsPerClient.Add(clientId, _deactivateFrozenTalentNetEventsListPool.Get());
            }
            if (!MoleSpawnedNetEventsPerClient.ContainsKey(clientId))
            {
                MoleSpawnedNetEventsPerClient.Add(clientId, _moleSpawnedNetEventsListPool.Get());
            }
            if (!MoleHitNetEventsPerClient.ContainsKey(clientId))
            {
                MoleHitNetEventsPerClient.Add(clientId, _moleHitNetEventsListPool.Get());
            }
            if (!MoleExpiredNetEventsPerClient.ContainsKey(clientId))
            {
                MoleExpiredNetEventsPerClient.Add(clientId, _moleExpiredNetEventsListPool.Get());
            }
            if (!GoldenMoleDamagedNetEventsPerClient.ContainsKey(clientId))
            {
                GoldenMoleDamagedNetEventsPerClient.Add(clientId, _goldenMoleDamagedNetEventsListPool.Get());
            }
            if (!ScoreGatePassedNetEventsPerClient.ContainsKey(clientId))
            {
                ScoreGatePassedNetEventsPerClient.Add(clientId, _scoreGatePassedNetEventsListPool.Get());
            }
            if (!GateTrapClosingNetEventsPerClient.ContainsKey(clientId))
            {
                GateTrapClosingNetEventsPerClient.Add(clientId, _gateTrapClosingNetEventsListPool.Get());
            }
            if (!CreateMagneticPullFieldNetEventsPerClient.ContainsKey(clientId))
            {
                CreateMagneticPullFieldNetEventsPerClient.Add(clientId, _createMagneticPullFieldNetEventsListPool.Get());
            }
            if (!ActivateUmbrellaTalentNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateUmbrellaTalentNetEventsPerClient.Add(clientId, _activateUmbrellaTalentNetEventsListPool.Get());
            }
            if (!DeactivateUmbrellaTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateUmbrellaTalentNetEventsPerClient.Add(clientId, _deactivateUmbrellaTalentNetEventsListPool.Get());
            }
            if (!ActivateWaterGunTalentNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateWaterGunTalentNetEventsPerClient.Add(clientId, _activateWaterGunTalentNetEventsListPool.Get());
            }
            if (!DeactivateWaterGunTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateWaterGunTalentNetEventsPerClient.Add(clientId, _deactivateWaterGunTalentNetEventsListPool.Get());
            }
            if (!ActivateHeadbuttChargingNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateHeadbuttChargingNetEventsPerClient.Add(clientId, _activateHeadbuttChargingNetEventsListPool.Get());
            }
            if (!PerformHeadbuttDashNetEventsPerClient.ContainsKey(clientId))
            {
                PerformHeadbuttDashNetEventsPerClient.Add(clientId, _performHeadbuttDashNetEventsListPool.Get());
            }
            if (!PerformBarrelDashNetEventsPerClient.ContainsKey(clientId))
            {
                PerformBarrelDashNetEventsPerClient.Add(clientId, _performBarrelDashNetEventsListPool.Get());
            }
            if (!HeadbuttHitEnemyNetEventsPerClient.ContainsKey(clientId))
            {
                HeadbuttHitEnemyNetEventsPerClient.Add(clientId, _headbuttHitEnemyNetEventsListPool.Get());
            }
            if (!DeactivateHeadbuttTalentNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateHeadbuttTalentNetEventsPerClient.Add(clientId, _deactivateHeadbuttTalentNetEventsListPool.Get());
            }
            if (!LayChickenEggNetEventsPerClient.ContainsKey(clientId))
            {
                LayChickenEggNetEventsPerClient.Add(clientId, _layChickenEggNetEventsPool.Get());
            }
            if (!ChickenEggHitNetEventsPerClient.ContainsKey(clientId))
            {
                ChickenEggHitNetEventsPerClient.Add(clientId, _chickenEggHitNetEventsPool.Get());
            }
            if (!ActivateYearsOfPainTalentNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateYearsOfPainTalentNetEventsPerClient.Add(clientId, _activateYearsOfPainTalentNetEventsListPool.Get());
            }
            if (!PerformGalacticPullNetEventsPerClient.ContainsKey(clientId))
            {
                PerformGalacticPullNetEventsPerClient.Add(clientId, _performGalacticPullNetEventsListPool.Get());
            }
            if (!DeactivateGalacticForceFieldNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateGalacticForceFieldNetEventsPerClient.Add(clientId, _deactivateGalacticForceFieldNetEventsListPool.Get());
            }
            if (!ActivateNukePowerUpNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateNukePowerUpNetEventsPerClient.Add(clientId, _activateNukePowerUpNetEventsListPool.Get());
            }
            if (!DeactivateShufflePowerUpNetEventsPerClient.ContainsKey(clientId))
            {
                DeactivateShufflePowerUpNetEventsPerClient.Add(clientId, _activateShufflePowerUpNetEventsListPool.Get());
            }
            if (!ShuffleSwapPlayerPositionNetEventsPerClient.ContainsKey(clientId))
            {
                ShuffleSwapPlayerPositionNetEventsPerClient.Add(clientId, _shuffleSwapPlayerPositionNetEventsListPool.Get());
            }
            if (!ActivateShuffleNetEventsPerClient.ContainsKey(clientId))
            {
                ActivateShuffleNetEventsPerClient.Add(clientId, _activateShuffleNetEventsListPool.Get());
            }
            if (!StartPowerUpGrantingPhaseNetEventsPerClient.ContainsKey(clientId))
            {
                StartPowerUpGrantingPhaseNetEventsPerClient.Add(clientId, _startPowerUpGrantingPhaseNetEventsListPool.Get());
            }
            if (!EndPowerUpGrantingPhaseNetEventsPerClient.ContainsKey(clientId))
            {
                EndPowerUpGrantingPhaseNetEventsPerClient.Add(clientId, _endPowerUpGrantingPhaseNetEventsListPool.Get());
            }
            if (!ShootFrigidBlockNetEventsPerClient.ContainsKey(clientId))
            {
                ShootFrigidBlockNetEventsPerClient.Add(clientId, _shootFrigidBlockNetEventsListPool.Get());
            }
            if (!DestroyFrigidBlockNetEventsPerClient.ContainsKey(clientId))
            {
                DestroyFrigidBlockNetEventsPerClient.Add(clientId, _destroyFrigidBlockNetEventsListPool.Get());
            }
        }

        public void StopSavingClientEvents(long clientId)
        {
            var bulletSpawnedList = BulletSpawnNetEventsPerClient[clientId];
            bulletSpawnedList.Clear();
            _bulletSpawnListPool.Return(bulletSpawnedList);
            var joinAcceptedList = PlayerRejoinAcceptNetEventsPerClient[clientId];
            joinAcceptedList.Clear();
            _playerRejoinAcceptListPool.Return(joinAcceptedList);
            var matchMakingJoinAcceptedList = MatchMakingPlayerJoinAcceptNetEventsPerClient[clientId];
            matchMakingJoinAcceptedList.Clear();
            _matchMakingPlayerJoinAcceptListPool.Return(matchMakingJoinAcceptedList);
            var playerTakeDamageedList = PlayerTakeDamageNetEventsPerClient[clientId];
            playerTakeDamageedList.Clear();
            _playerTakeDamageListPool.Return(playerTakeDamageedList);
            var playerDiedList = PlayerDiedNetEventsPerClient[clientId];
            playerDiedList.Clear();
            _playerDiedListPool.Return(playerDiedList);
            var bulletDestroyededList = BulletDestroyedNetEventsPerClient[clientId];
            bulletDestroyededList.Clear();
            _bulletDestroyedListPool.Return(bulletDestroyededList);
            var playerSwapList = PlayerSwapNetEventsPerClient[clientId];
            playerSwapList.Clear();
            _playerSwapListPool.Return(playerSwapList);
            var talentCardObtainedList = TalentCardObtainedNetEventsPerClient[clientId];
            talentCardObtainedList.Clear();
            _talentCardObtainedListPool.Return(talentCardObtainedList);
            var talentCardHitList = TalentCardHitNetEventsPerClient[clientId];
            talentCardHitList.Clear();
            _talentCardHitListPool.Return(talentCardHitList);

            var playerSpinnedList = PlayerSpinnedStartedNetEventsPerClient[clientId];
            playerSpinnedList.Clear();
            _playerSpinnedStartedListPool.Return(playerSpinnedList);
            var playerSpinnedEndedList = PlayerSpinnedEndedNetEventsPerClient[clientId];
            playerSpinnedEndedList.Clear();
            _playerSpinnedEndedListPool.Return(playerSpinnedEndedList);
            var playerStartedExposedToLavaList = PlayerStartedExposedToLavaNetEventsPerClient[clientId];
            playerStartedExposedToLavaList.Clear();
            _playerStartedExposedToLavaListPool.Return(playerStartedExposedToLavaList);
            var playerEndedExposedToLavaList = PlayerEndedExposedToLavaNetEventsPerClient[clientId];
            playerEndedExposedToLavaList.Clear();
            _playerEndedExposedToLavaListPool.Return(playerEndedExposedToLavaList);
            var powerUpBallsSpawnedList = PowerUpBallSpawnedNetEventsPerClient[clientId];
            powerUpBallsSpawnedList.Clear();
            _powerUpBallsSpawnedListPool.Return(powerUpBallsSpawnedList);
            var powerUpBallsObtainedList = PowerUpBallObtainedNetEventsPerClient[clientId];
            powerUpBallsObtainedList.Clear();
            _powerUpBallsObtainedListPool.Return(powerUpBallsObtainedList);
            var playerSwitchTeamList = PlayerSwitchTeamNetEventsPerClient[clientId];
            playerSwitchTeamList.Clear();
            _playerSwitchTeamListPool.Return(playerSwitchTeamList);
            var startMatchCountdownList = StartMatchCountdownNetEventsPerClient[clientId];
            startMatchCountdownList.Clear();
            _startMatchCountdownListPool.Return(startMatchCountdownList);
            var stopMatchCountdownList = StopMatchCountdownNetEventsPerClient[clientId];
            stopMatchCountdownList.Clear();
            _stopMatchCountdownListPool.Return(stopMatchCountdownList);
            var stageEndList = StageEndNetEventsPerClient[clientId];
            stageEndList.Clear();
            _stageEndNetEventsListPool.Return(stageEndList);
            var playerLockOnTargetsChangedList = PlayerLockOnTargetsChangedNetEventsPerClient[clientId];
            playerLockOnTargetsChangedList.Clear();
            _playerLockOnTargetsChangedNetEventsListPool.Return(playerLockOnTargetsChangedList);
            var playerLockOnTargetHitList = PlayerLockedOnTargetHitNetEventsPerClient[clientId];
            playerLockOnTargetHitList.Clear();
            _playerLockOnTargetHitNetEventsListPool.Return(playerLockOnTargetHitList);
            var playerPowerUpChangedList = PlayerPowerUpChangedNetEventsPerClient[clientId];
            playerPowerUpChangedList.Clear();
            _playerPowerUpChangedNetEventsListPool.Return(playerPowerUpChangedList);
            var sonicSlapActivatedList = ActivateSonicSlapNetEventsPerClient[clientId];
            sonicSlapActivatedList.Clear();
            _sonicSlapActivatedNetEventsListPool.Return(sonicSlapActivatedList);
            var teamLostList = TeamLostNetEventsPerClient[clientId];
            teamLostList.Clear();
            _teamLostNetEventsListPool.Return(teamLostList);
            var talentSwitchList = TalentSwitchNetEventsPerClient[clientId];
            talentSwitchList.Clear();
            _talentSwitchNetEventsListPool.Return(talentSwitchList);
            var startMatchEligibleChangedList = StartMatchEligibleChangedNetEventsPerClient[clientId];
            startMatchEligibleChangedList.Clear();
            _startMatchEligibleChangedListPool.Return(startMatchEligibleChangedList);
            var environmentSpringPlayerCollisionList = EnvironmentSpringPlayerCollisionNetEventsPerClient[clientId];
            environmentSpringPlayerCollisionList.Clear();
            _environmentSpringPlayerCollisionListPool.Return(environmentSpringPlayerCollisionList);
            var environmentSpikePlayerCollisionList = EnvironmentSpikePlayerCollisionNetEventsPerClient[clientId];
            environmentSpikePlayerCollisionList.Clear();
            _environmentSpikePlayerCollisionListPool.Return(environmentSpikePlayerCollisionList);
            var gainBoltsList = GainBoltsNetEventsPerClient[clientId];
            gainBoltsList.Clear();
            _gainBoltsNetEventsListPool.Return(gainBoltsList);
            var playerToEnvironmentTeleportGateCollisionList = PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient[clientId];
            playerToEnvironmentTeleportGateCollisionList.Clear();
            _playerToEnvironmentTeleportGateCollisionListPool.Return(playerToEnvironmentTeleportGateCollisionList);
            var preparationPhaseEndedList = PreparationPhaseEndedNetEventsPerClient[clientId];
            preparationPhaseEndedList.Clear();
            _preparationPhaseEndedListPool.Return(preparationPhaseEndedList);
            var createSwapFieldNetEventsList = CreateSwapFieldNetEventsPerClient[clientId];
            createSwapFieldNetEventsList.Clear();
            _createSwapFieldNetEventsListPool.Return(createSwapFieldNetEventsList);
            var deactivateSwapTalentNetEventsList = DeactivateSwapTalentNetEventsPerClient[clientId];
            deactivateSwapTalentNetEventsList.Clear();
            _deactivateSwapTalentNetEventsListPool.Return(deactivateSwapTalentNetEventsList);

            var createKOProjectileNetEventsList = CreateKOProjectileNetEventsPerClient[clientId];
            createKOProjectileNetEventsList.Clear();
            _createKOProjectileNetEventsListPool.Return(createKOProjectileNetEventsList);

            var koProjectHitPlayerNetEventsList = KOProjectHitPlayerNetEventsPerClient[clientId];
            koProjectHitPlayerNetEventsList.Clear();
            _koProjectHitPlayerNetEventsListPool.Return(koProjectHitPlayerNetEventsList);

            var deactivateKOTalentNetEventsList = DeactivateKOTalentNetEventsPerClient[clientId];
            deactivateKOTalentNetEventsList.Clear();
            _deactivateKOTalentNetEventsListPool.Return(deactivateKOTalentNetEventsList);
            
            var performDashPulseNetEventsList = PerformDashPulseNetEventsPerClient[clientId];
            performDashPulseNetEventsList.Clear();
            _performDashPulseNetEventsListPool.Return(performDashPulseNetEventsList);

            var activateSentryGunTalentNetEventsList = ActivateSentryGunTalentNetEventsPerClient[clientId];
            activateSentryGunTalentNetEventsList.Clear();
            _activateSentryGunTalentNetEventsListPool.Return(activateSentryGunTalentNetEventsList);

            var deactivateSentryGunTalentNetEventsList = DeactivateSentryGunTalentNetEventsPerClient[clientId];
            deactivateSentryGunTalentNetEventsList.Clear();
            _deactivateSentryGunTalentNetEventsListPool.Return(deactivateSentryGunTalentNetEventsList);
            
            var updatePlayerTalentStocksNetEventsList = UpdatePlayerTalentStocksNetEventsPerClient[clientId];
            updatePlayerTalentStocksNetEventsList.Clear();
            _updatePlayerTalentStocksNetEventsListPool.Return(updatePlayerTalentStocksNetEventsList);
            
            var playerMaxShootCooldownChangedList = PlayerMaxShootCooldownChangedNetEventsPerClient[clientId];
            playerMaxShootCooldownChangedList.Clear();
            _playerMaxShootCooldownChangedListPool.Return(playerMaxShootCooldownChangedList);

            var createGrapplingHookProjectileNetEventsList = CreateGrapplingHookProjectileNetEventsPerClient[clientId];
            createGrapplingHookProjectileNetEventsList.Clear();
            _createGrapplingHookProjectileNetEventsListPool.Return(createGrapplingHookProjectileNetEventsList);

            var grapplingHookHitWallNetEventsList = GrapplingHookHitWallNetEventsPerClient[clientId];
            grapplingHookHitWallNetEventsList.Clear();
            _grapplingHookHitWallNetEventsListPool.Return(grapplingHookHitWallNetEventsList);

            var deactivateGrapplingHookTalentNetEventsList = DeactivateGrapplingHookTalentNetEventsPerClient[clientId];
            deactivateGrapplingHookTalentNetEventsList.Clear();
            _deactivateGrapplingHookTalentNetEventsListPool.Return(deactivateGrapplingHookTalentNetEventsList);

            var createFishingRodProjectileNetEventsList = CreateFishingRodProjectileNetEventsPerClient[clientId];
            createFishingRodProjectileNetEventsList.Clear();
            _createFishingRodProjectileNetEventsListPool.Return(createFishingRodProjectileNetEventsList);

            var fishingRodCaughtEnemyNetEventsList = FishingRodCaughtEnemyNetEventsPerClient[clientId];
            fishingRodCaughtEnemyNetEventsList.Clear();
            _fishingRodCaughtEnemyNetEventsListPool.Return(fishingRodCaughtEnemyNetEventsList);

            var fishingRodTipHitWallNetEventsList = FishingRodTipHitWallNetEventsPerClient[clientId];
            fishingRodTipHitWallNetEventsList.Clear();
            _fishingRodTipHitWallNetEventsListPool.Return(fishingRodTipHitWallNetEventsList);

            var fishingRodThrowNetEventsList = FishingRodThrowNetEventsPerClient[clientId];
            fishingRodThrowNetEventsList.Clear();
            _fishingRodThrowNetEventsListPool.Return(fishingRodThrowNetEventsList);

            var deactivateFishingRodTalentNetEventsList = DeactivateFishingRodTalentNetEventsPerClient[clientId];
            deactivateFishingRodTalentNetEventsList.Clear();
            _deactivateFishingRodTalentNetEventsListPool.Return(deactivateFishingRodTalentNetEventsList);

            var createSoulGhostNetEventsList = CreateSoulGhostNetEventsPerClient[clientId];
            createSoulGhostNetEventsList.Clear();
            _createSoulGhostNetEventsListPool.Return(createSoulGhostNetEventsList);

            var deactivateSoulTalentNetEventsList = DeactivateSoulTalentNetEventsPerClient[clientId];
            deactivateSoulTalentNetEventsList.Clear();
            _deactivateSoulTalentNetEventsListPool.Return(deactivateSoulTalentNetEventsList);

            var activateRockTalentNetEventsList = ActivateRockTalentNetEventsPerClient[clientId];
            activateRockTalentNetEventsList.Clear();
            _activateRockTalentNetEventsListPool.Return(activateRockTalentNetEventsList);

            var deactivateRockTalentNetEventsList = DeactivateRockTalentNetEventsPerClient[clientId];
            deactivateRockTalentNetEventsList.Clear();
            _deactivateRockTalentNetEventsListPool.Return(deactivateRockTalentNetEventsList);

            var activateFrozenTalentNetEventsList = ActivateFrozenTalentNetEventsPerClient[clientId];
            activateFrozenTalentNetEventsList.Clear();
            _activateFrozenTalentNetEventsListPool.Return(activateFrozenTalentNetEventsList);

            var deactivateFrozenTalentNetEventsList = DeactivateFrozenTalentNetEventsPerClient[clientId];
            deactivateFrozenTalentNetEventsList.Clear();
            _deactivateFrozenTalentNetEventsListPool.Return(deactivateFrozenTalentNetEventsList);

            var moleSpawnedNetEventsList = MoleSpawnedNetEventsPerClient[clientId];
            moleSpawnedNetEventsList.Clear();
            _moleSpawnedNetEventsListPool.Return(moleSpawnedNetEventsList);

            var moleHitNetEventsList = MoleHitNetEventsPerClient[clientId];
            moleHitNetEventsList.Clear();
            _moleHitNetEventsListPool.Return(moleHitNetEventsList);

            var moleExpiredNetEventsList = MoleExpiredNetEventsPerClient[clientId];
            moleExpiredNetEventsList.Clear();
            _moleExpiredNetEventsListPool.Return(moleExpiredNetEventsList);

            var goldenMoleDamagedNetEventsList = GoldenMoleDamagedNetEventsPerClient[clientId];
            goldenMoleDamagedNetEventsList.Clear();
            _goldenMoleDamagedNetEventsListPool.Return(goldenMoleDamagedNetEventsList);

            var scoreGatePassedNetEventsList = ScoreGatePassedNetEventsPerClient[clientId];
            scoreGatePassedNetEventsList.Clear();
            _scoreGatePassedNetEventsListPool.Return(scoreGatePassedNetEventsList);

            var gateTrapClosingNetEventsList = GateTrapClosingNetEventsPerClient[clientId];
            gateTrapClosingNetEventsList.Clear();
            _gateTrapClosingNetEventsListPool.Return(gateTrapClosingNetEventsList);

            var createMagneticPullFieldNetEventsList = CreateMagneticPullFieldNetEventsPerClient[clientId];
            createMagneticPullFieldNetEventsList.Clear();
            _createMagneticPullFieldNetEventsListPool.Return(createMagneticPullFieldNetEventsList);

            CreateMagneticPullFieldNetEventsPerClient.Remove(clientId);
            var activateUmbrellaTalentNetEventsList = ActivateUmbrellaTalentNetEventsPerClient[clientId];
            activateUmbrellaTalentNetEventsList.Clear();
            _activateUmbrellaTalentNetEventsListPool.Return(activateUmbrellaTalentNetEventsList);

            var deactivateUmbrellaTalentNetEventsList = DeactivateUmbrellaTalentNetEventsPerClient[clientId];
            deactivateUmbrellaTalentNetEventsList.Clear();
            _deactivateUmbrellaTalentNetEventsListPool.Return(deactivateUmbrellaTalentNetEventsList);

            var activateWaterGunTalentNetEventsList = ActivateWaterGunTalentNetEventsPerClient[clientId];
            activateWaterGunTalentNetEventsList.Clear();
            _activateWaterGunTalentNetEventsListPool.Return(activateWaterGunTalentNetEventsList);

            var deactivateWaterGunTalentNetEventsList = DeactivateWaterGunTalentNetEventsPerClient[clientId];
            deactivateWaterGunTalentNetEventsList.Clear();
            _deactivateWaterGunTalentNetEventsListPool.Return(deactivateWaterGunTalentNetEventsList);

            var activateHeadbuttChargingList = ActivateHeadbuttChargingNetEventsPerClient[clientId];
            activateHeadbuttChargingList.Clear();
            _activateHeadbuttChargingNetEventsListPool.Return(activateHeadbuttChargingList);

            var performHeadbuttDashList = PerformHeadbuttDashNetEventsPerClient[clientId];
            performHeadbuttDashList.Clear();
            _performHeadbuttDashNetEventsListPool.Return(performHeadbuttDashList);

            var performBarrelDashList = PerformBarrelDashNetEventsPerClient[clientId];
            performBarrelDashList.Clear();
            _performBarrelDashNetEventsListPool.Return(performBarrelDashList);

            var headbuttHitEnemyList = HeadbuttHitEnemyNetEventsPerClient[clientId];
            headbuttHitEnemyList.Clear();
            _headbuttHitEnemyNetEventsListPool.Return(headbuttHitEnemyList);

            var deactivateHeadbuttTalentList = DeactivateHeadbuttTalentNetEventsPerClient[clientId];
            deactivateHeadbuttTalentList.Clear();
            _deactivateHeadbuttTalentNetEventsListPool.Return(deactivateHeadbuttTalentList);

            var layChickenEggNetEventsList = LayChickenEggNetEventsPerClient[clientId];
            layChickenEggNetEventsList.Clear();
            _layChickenEggNetEventsPool.Return(layChickenEggNetEventsList);
            
            var chickenEggHitNetEventsList = ChickenEggHitNetEventsPerClient[clientId];
            chickenEggHitNetEventsList.Clear();
            _chickenEggHitNetEventsPool.Return(chickenEggHitNetEventsList);

            var activateYearsOfPainTalentNetEventsList = ActivateYearsOfPainTalentNetEventsPerClient[clientId];
            activateYearsOfPainTalentNetEventsList.Clear();
            _activateYearsOfPainTalentNetEventsListPool.Return(activateYearsOfPainTalentNetEventsList);

            var performGalacticPullNetEventsList = PerformGalacticPullNetEventsPerClient[clientId];
            performGalacticPullNetEventsList.Clear();
            _performGalacticPullNetEventsListPool.Return(performGalacticPullNetEventsList);

            var deactivateGalacticForceFieldNetEventsList = DeactivateGalacticForceFieldNetEventsPerClient[clientId];
            deactivateGalacticForceFieldNetEventsList.Clear();
            _deactivateGalacticForceFieldNetEventsListPool.Return(deactivateGalacticForceFieldNetEventsList);

            var activateNukePowerUpNetEventsList = ActivateNukePowerUpNetEventsPerClient[clientId];
            activateNukePowerUpNetEventsList.Clear();
            _activateNukePowerUpNetEventsListPool.Return(activateNukePowerUpNetEventsList);

            var activateShufflePowerUpNetEventsList = DeactivateShufflePowerUpNetEventsPerClient[clientId];
            activateShufflePowerUpNetEventsList.Clear();
            _activateShufflePowerUpNetEventsListPool.Return(activateShufflePowerUpNetEventsList);

            var startPowerUpGrantingPhaseNetEventsList = StartPowerUpGrantingPhaseNetEventsPerClient[clientId];
            startPowerUpGrantingPhaseNetEventsList.Clear();
            _startPowerUpGrantingPhaseNetEventsListPool.Return(startPowerUpGrantingPhaseNetEventsList);
            StartPowerUpGrantingPhaseNetEventsPerClient.Remove(clientId);

            var endPowerUpGrantingPhaseNetEventsList = EndPowerUpGrantingPhaseNetEventsPerClient[clientId];
            endPowerUpGrantingPhaseNetEventsList.Clear();
            _endPowerUpGrantingPhaseNetEventsListPool.Return(endPowerUpGrantingPhaseNetEventsList);
            EndPowerUpGrantingPhaseNetEventsPerClient.Remove(clientId);

            var shootFrigidBlockNetEventsList = ShootFrigidBlockNetEventsPerClient[clientId];
            shootFrigidBlockNetEventsList.Clear();
            _shootFrigidBlockNetEventsListPool.Return(shootFrigidBlockNetEventsList);
            ShootFrigidBlockNetEventsPerClient.Remove(clientId);

            var destroyFrigidBlockNetEventsList = DestroyFrigidBlockNetEventsPerClient[clientId];
            destroyFrigidBlockNetEventsList.Clear();
            _destroyFrigidBlockNetEventsListPool.Return(destroyFrigidBlockNetEventsList);
            DestroyFrigidBlockNetEventsPerClient.Remove(clientId);

            BulletSpawnNetEventsPerClient.Remove(clientId);
            PlayerRejoinAcceptNetEventsPerClient.Remove(clientId);
            MatchMakingPlayerJoinAcceptNetEventsPerClient.Remove(clientId);
            PlayerTakeDamageNetEventsPerClient.Remove(clientId);
            PlayerDiedNetEventsPerClient.Remove(clientId);
            BulletDestroyedNetEventsPerClient.Remove(clientId);
            PlayerSwapNetEventsPerClient.Remove(clientId);
            TalentCardObtainedNetEventsPerClient.Remove(clientId);
            TalentCardHitNetEventsPerClient.Remove(clientId);
            PlayerSpinnedStartedNetEventsPerClient.Remove(clientId);
            PlayerSpinnedEndedNetEventsPerClient.Remove(clientId);
            PlayerStartedExposedToLavaNetEventsPerClient.Remove(clientId);
            PlayerEndedExposedToLavaNetEventsPerClient.Remove(clientId);
            PowerUpBallSpawnedNetEventsPerClient.Remove(clientId);
            PowerUpBallObtainedNetEventsPerClient.Remove(clientId);
            PlayerSwitchTeamNetEventsPerClient.Remove(clientId);
            StartMatchCountdownNetEventsPerClient.Remove(clientId);
            StopMatchCountdownNetEventsPerClient.Remove(clientId);
            StageEndNetEventsPerClient.Remove(clientId);
            PlayerLockOnTargetsChangedNetEventsPerClient.Remove(clientId);
            PlayerLockedOnTargetHitNetEventsPerClient.Remove(clientId);
            PlayerPowerUpChangedNetEventsPerClient.Remove(clientId);
            ActivateSonicSlapNetEventsPerClient.Remove(clientId);
            TeamLostNetEventsPerClient.Remove(clientId);
            TalentSwitchNetEventsPerClient.Remove(clientId);
            StartMatchEligibleChangedNetEventsPerClient.Remove(clientId);
            EnvironmentSpringPlayerCollisionNetEventsPerClient.Remove(clientId);
            EnvironmentSpikePlayerCollisionNetEventsPerClient.Remove(clientId);
            GainBoltsNetEventsPerClient.Remove(clientId);
            PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient.Remove(clientId);
            PreparationPhaseEndedNetEventsPerClient.Remove(clientId);
            CreateSwapFieldNetEventsPerClient.Remove(clientId);
            DeactivateSwapTalentNetEventsPerClient.Remove(clientId);
            CreateKOProjectileNetEventsPerClient.Remove(clientId);
            KOProjectHitPlayerNetEventsPerClient.Remove(clientId);
            DeactivateKOTalentNetEventsPerClient.Remove(clientId);
            PerformDashPulseNetEventsPerClient.Remove(clientId);
            ActivateSentryGunTalentNetEventsPerClient.Remove(clientId);
            DeactivateSentryGunTalentNetEventsPerClient.Remove(clientId);
            UpdatePlayerTalentStocksNetEventsPerClient.Remove(clientId);
            PlayerMaxShootCooldownChangedNetEventsPerClient.Remove(clientId);
            CreateGrapplingHookProjectileNetEventsPerClient.Remove(clientId);
            GrapplingHookHitWallNetEventsPerClient.Remove(clientId);
            DeactivateGrapplingHookTalentNetEventsPerClient.Remove(clientId);
            CreateFishingRodProjectileNetEventsPerClient.Remove(clientId);
            FishingRodCaughtEnemyNetEventsPerClient.Remove(clientId);
            FishingRodTipHitWallNetEventsPerClient.Remove(clientId);
            FishingRodThrowNetEventsPerClient.Remove(clientId);
            DeactivateFishingRodTalentNetEventsPerClient.Remove(clientId);
            CreateSoulGhostNetEventsPerClient.Remove(clientId);
            DeactivateSoulTalentNetEventsPerClient.Remove(clientId);
            ActivateRockTalentNetEventsPerClient.Remove(clientId);
            DeactivateRockTalentNetEventsPerClient.Remove(clientId);
            ActivateFrozenTalentNetEventsPerClient.Remove(clientId);
            DeactivateFrozenTalentNetEventsPerClient.Remove(clientId);
            MoleSpawnedNetEventsPerClient.Remove(clientId);
            MoleHitNetEventsPerClient.Remove(clientId);
            MoleExpiredNetEventsPerClient.Remove(clientId);
            GoldenMoleDamagedNetEventsPerClient.Remove(clientId);
            ScoreGatePassedNetEventsPerClient.Remove(clientId);
            GateTrapClosingNetEventsPerClient.Remove(clientId);
            ActivateUmbrellaTalentNetEventsPerClient.Remove(clientId);
            DeactivateUmbrellaTalentNetEventsPerClient.Remove(clientId);
            ActivateWaterGunTalentNetEventsPerClient.Remove(clientId);
            DeactivateWaterGunTalentNetEventsPerClient.Remove(clientId);
            ActivateHeadbuttChargingNetEventsPerClient.Remove(clientId);
            PerformHeadbuttDashNetEventsPerClient.Remove(clientId);
            PerformBarrelDashNetEventsPerClient.Remove(clientId);
            HeadbuttHitEnemyNetEventsPerClient.Remove(clientId);
            DeactivateHeadbuttTalentNetEventsPerClient.Remove(clientId);
            LayChickenEggNetEventsPerClient.Remove(clientId);
            ChickenEggHitNetEventsPerClient.Remove(clientId);
            ActivateYearsOfPainTalentNetEventsPerClient.Remove(clientId);
            PerformGalacticPullNetEventsPerClient.Remove(clientId);
            DeactivateGalacticForceFieldNetEventsPerClient.Remove(clientId);
            ActivateNukePowerUpNetEventsPerClient.Remove(clientId);
            DeactivateShufflePowerUpNetEventsPerClient.Remove(clientId);

            var shuffleSwapPlayerPositionNetEventsList = ShuffleSwapPlayerPositionNetEventsPerClient[clientId];
            shuffleSwapPlayerPositionNetEventsList.Clear();
            _shuffleSwapPlayerPositionNetEventsListPool.Return(shuffleSwapPlayerPositionNetEventsList);
            ShuffleSwapPlayerPositionNetEventsPerClient.Remove(clientId);

            var activateShuffleNetEventsList = ActivateShuffleNetEventsPerClient[clientId];
            activateShuffleNetEventsList.Clear();
            _activateShuffleNetEventsListPool.Return(activateShuffleNetEventsList);
            ActivateShuffleNetEventsPerClient.Remove(clientId);
        }
        
        public void AddPlayerTakeDamageNetEvent(int onTick, ushort damagedPlayerId, ushort playerHealth, ushort hitDamage, bool isAlive)
        {
            foreach (var kvp in PlayerTakeDamageNetEventsPerClient)
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
            foreach (var kvp in PlayerDiedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
            }
        }

        public void AddPlayerMaxShootCooldownChangedNetEvent(int onTick, ushort playerId, float maxShootCooldown, float shootCooldownSecondsLeft)
        {
            foreach (var kvp in PlayerMaxShootCooldownChangedNetEventsPerClient)
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
            foreach (var kvp in BulletDestroyedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.BulletId = bulletId;
                packet.Position = position;
            }
        }

        public void AddBulletSpawnNetEvent(int onTick, ushort bulletId, ushort belongToPlayerId, Vector2 position, float bulletRadius, Vector2 velocity)
        {
            foreach (var kvp in BulletSpawnNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.BulletId = bulletId;
                packet.BelongToPlayerId = belongToPlayerId;
                packet.Position = position;
                packet.BulletRadius = bulletRadius;
                packet.Velocity = velocity;
            }
        }

        public void AddClientJoinAcceptedEvent(int onTick, List<PlayerStateS2C> playerStates, MatchSimulationStateS2C simulationState, long clientId)
        {
            foreach (var kvp in PlayerRejoinAcceptNetEventsPerClient)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.IsLocal = clientId == kvp.Key;
                packet.SimulationState = simulationState;
                
                packet.Players.Clear();
                for (int i = 0; i < playerStates.Count; i++)
                {
                    packet.Players.AddAndGet();
                    packet.Players[i] = playerStates[i];
                }
            }
        }

        public void AddMatchMakingClientJoinAcceptedEvent(int onTick, List<MatchMakingPlayerStateS2C> playerStates, MatchMakingSimulationStateS2C simulationState, long clientId)
        {
            foreach (var kvp in MatchMakingPlayerJoinAcceptNetEventsPerClient)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.IsLocal = clientId == kvp.Key;
                packet.SimulationState = simulationState;
                
                packet.Players.Clear();
                for (int i = 0; i < playerStates.Count; i++)
                {
                    packet.Players.AddAndGet();
                    packet.Players[i] = playerStates[i];
                }
            }
        }

        public void AddPlayersSwapEvent(int onTick, ushort casterPlayerId, ushort otherPlayerId, Vector2 casterPlayerPosition, Vector2 otherPlayerPosition, Vector2 casterPlayerDirection,
            Vector2 otherPlayerDirection)
        {
            foreach (var kvp in PlayerSwapNetEventsPerClient)
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
            foreach (var kvp in TalentCardObtainedNetEventsPerClient)
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
            foreach (var kvp in TalentCardHitNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.TalentCardId = cardId;
                packet.TalentCardHealth = cardHealth;
            }
        }

        public void AddPowerUpSpawnedNetEvent(int onTick, ushort powerUpBallId, Vector2 position)
        {
            foreach (var kvp in PowerUpBallSpawnedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PowerUpBallId = powerUpBallId;
                packet.Position = position;
            }
        }

        public void AddPowerUpObtainedNetEvent(int onTick, ushort powerUpBallId, ushort byPlayerId)
        {
            foreach (var kvp in PowerUpBallObtainedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.Id = powerUpBallId;
                packet.ObtainedByPlayerId = byPlayerId;
            }
        }

        public void AddPlayerSwitchTeamNetEvent(int onTick, ushort playerId, ushort teamId)
        {
            foreach (var kvp in PlayerSwitchTeamNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.TeamId = teamId;
            }
        }
        
        public void AddPlayerLockOnTargetsChangedNetEvent(int onTick, ushort playerId, FixedUnorderedList<ObjectLockedOnTargetS2C> playerIdsLockedOnTarget)
        {
            foreach (var kvp in PlayerLockOnTargetsChangedNetEventsPerClient)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.LockedOnTargetObjects.Clear();
                foreach (var playerLockedOnTarget in playerIdsLockedOnTarget.AsSpan())
                {
                    ref var playerlockOnId = ref packet.LockedOnTargetObjects.AddAndGet();
                    playerlockOnId = playerLockedOnTarget;
                }
            }
        }

        public void AddPlayerLockedOnTargetHitNetEvent(int onTick, ushort casterPlayId, ushort hitPlayerId)
        {
            foreach (var kvp in PlayerLockedOnTargetHitNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.CasterPlayerId = casterPlayId;
                netEvent.HitPlayerId = hitPlayerId;
            }
        }

        public void AddPlayerPowerUpChangedNetEvent(int onTick, ushort playerId, PowerUpType powerUp)
        {
            foreach (var kvp in PlayerPowerUpChangedNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.PlayerId = playerId;
                netEvent.PowerUp = powerUp;
            }
        }

        public void AddActivateSonicSlapNetEvent(int onTick, ushort casterPlayerId, FixedUnorderedList<ushort> affectedPlayerIds)
        {
            foreach (var kvp in ActivateSonicSlapNetEventsPerClient)
            {
                var netEvent = kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.CasterPlayerId = casterPlayerId;
                netEvent.AffectedPlayerIds.Clear();
                foreach (var affectedPlayerId in affectedPlayerIds.AsSpan())
                {
                    ref var affectedId = ref netEvent.AffectedPlayerIds.AddAndGet();
                    affectedId = affectedPlayerId;
                }
            }
        }

        public void AddPerformGalacticPullNetEvent(int onTick, ushort fieldId, ushort casterPlayerId, ushort casterTeamId)
        {
            foreach (var kvp in PerformGalacticPullNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.FieldId = fieldId;
                netEvent.CasterPlayerId = casterPlayerId;
                netEvent.CasterTeamId = casterTeamId;
            }
        }

        public void AddDeactivateGalacticForceFieldNetEvent(int onTick, ushort galacticForceFieldId)
        {
            foreach (var kvp in DeactivateGalacticForceFieldNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.GalacticForceFieldId = galacticForceFieldId;
            }
        }

        public void AddActivateNukePowerUpNetEvent(int onTick, ushort casterPlayerId, Vector2 casterPosition)
        {
            foreach (var kvp in ActivateNukePowerUpNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.CasterPlayerId = casterPlayerId;
                netEvent.CasterPosition = casterPosition;
            }
        }

        public void AddDectivateShufflePowerUpNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in DeactivateShufflePowerUpNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddShuffleSwapPlayerPositionNetEvent(int onTick)
        {
            foreach (var kvp in ShuffleSwapPlayerPositionNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
            }
        }

        public void AddActivateShuffleNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in ActivateShuffleNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddStartPowerUpGrantingPhaseNetEvent(int onTick, ushort playerId)
        {
            foreach (var kvp in StartPowerUpGrantingPhaseNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.PlayerId = playerId;
            }
        }

        public void AddEndPowerUpGrantingPhaseNetEvent(int onTick, ushort playerId, PowerUpType grantedPowerUp)
        {
            foreach (var kvp in EndPowerUpGrantingPhaseNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.PlayerId = playerId;
                netEvent.GrantedPowerUp = grantedPowerUp;
            }
        }

        public void AddShootFrigidBlockNetEvent(int onTick, TalentFrigidBlockStateS2C frigidBlock, int cooldownEndTick)
        {
            foreach (var kvp in ShootFrigidBlockNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.FrigidBlock = frigidBlock;
                netEvent.CooldownEndTick = cooldownEndTick;
            }
        }

        public void AddDestroyFrigidBlockNetEvent(int onTick, ushort blockId)
        {
            foreach (var kvp in DestroyFrigidBlockNetEventsPerClient)
            {
                ref var netEvent = ref kvp.Value.AddAndGet();
                netEvent.OccuredOnTick = onTick;
                netEvent.BlockId = blockId;
            }
        }

        public void RemoveAllEventsOlderThanTick(long clientId, int tick)
        {
            if (BulletSpawnNetEventsPerClient.TryGetValue(clientId, out var bulletSpawnNetEvents))
            {
                for (int i = bulletSpawnNetEvents.Count - 1; i >= 0; i--)
                {
                    if(bulletSpawnNetEvents[i].OccuredOnTick<tick)
                    {
                        bulletSpawnNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerSpinnedStartedNetEventsPerClient.TryGetValue(clientId, out var playerSpinnedNetEvents))
            {
                for (var i = playerSpinnedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerSpinnedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerSpinnedNetEvents.RemoveAt(i);
                    }
                }
            }
            if (PlayerSpinnedEndedNetEventsPerClient.TryGetValue(clientId, out var playerSpinnedEndedNetEvents))
            {
                for (var i = playerSpinnedEndedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerSpinnedEndedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerSpinnedEndedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerStartedExposedToLavaNetEventsPerClient.TryGetValue(clientId, out var playerStartedExposedToLavaNetEvents))
            {
                for (var i = playerStartedExposedToLavaNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerStartedExposedToLavaNetEvents[i].OccuredOnTick < tick)
                    {
                        playerStartedExposedToLavaNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerEndedExposedToLavaNetEventsPerClient.TryGetValue(clientId, out var playerEndedExposedToLavaNetEvents))
            {
                for (var i = playerEndedExposedToLavaNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerEndedExposedToLavaNetEvents[i].OccuredOnTick < tick)
                    {
                        playerEndedExposedToLavaNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerRejoinAcceptNetEventsPerClient.TryGetValue(clientId, out var joinAcceptNetEvents))
            {
                for (int i = joinAcceptNetEvents.Count - 1; i >= 0; i--)
                {
                    if(joinAcceptNetEvents[i].OccuredOnTick < tick)
                    {
                        joinAcceptNetEvents.RemoveAt(i);
                    }
                }
            } 
            
            if (MatchMakingPlayerJoinAcceptNetEventsPerClient.TryGetValue(clientId, out var makingPlayerJoinAcceptNetEvents))
            {
                for (int i = makingPlayerJoinAcceptNetEvents.Count - 1; i >= 0; i--)
                {
                    if(makingPlayerJoinAcceptNetEvents[i].OccuredOnTick < tick)
                    {
                        makingPlayerJoinAcceptNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerTakeDamageNetEventsPerClient.TryGetValue(clientId, out var playerTakeDamageNetEvents))
            {
                for (int i = playerTakeDamageNetEvents.Count - 1; i >= 0; i--)
                {
                    if(playerTakeDamageNetEvents[i].OccuredOnTick < tick)
                    {
                        playerTakeDamageNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerDiedNetEventsPerClient.TryGetValue(clientId, out var playerDiedNetEvents))
            {
                for (int i = playerDiedNetEvents.Count - 1; i >= 0; i--)
                {
                    if(playerDiedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerDiedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (BulletDestroyedNetEventsPerClient.TryGetValue(clientId, out var bulletDestroyedNetEvents))
            {
                for (int i = bulletDestroyedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (bulletDestroyedNetEvents[i].OccuredOnTick < tick)
                    {
                        bulletDestroyedNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PlayerSwapNetEventsPerClient.TryGetValue(clientId, out var playerSwapNetEvents))
            {
                for (int i = playerSwapNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerSwapNetEvents[i].OccuredOnTick < tick)
                    {
                        playerSwapNetEvents.RemoveAt(i);
                    }
                }
            }

            if (TalentCardObtainedNetEventsPerClient.TryGetValue(clientId, out var talentCardObtainedNetEvents))
            {
                for (int i = talentCardObtainedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (talentCardObtainedNetEvents[i].OccuredOnTick < tick)
                    {
                        talentCardObtainedNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (TalentCardHitNetEventsPerClient.TryGetValue(clientId, out var talentCardHitNetEvents))
            {
                for (int i = talentCardHitNetEvents.Count - 1; i >= 0; i--)
                {
                    if (talentCardHitNetEvents[i].OccuredOnTick < tick)
                    {
                        talentCardHitNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PlayerSwitchTeamNetEventsPerClient.TryGetValue(clientId, out var playerSwitchTeamNetEvents))
            {
                for (int i = playerSwitchTeamNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerSwitchTeamNetEvents[i].OccuredOnTick < tick)
                    {
                        playerSwitchTeamNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PowerUpBallSpawnedNetEventsPerClient.TryGetValue(clientId, out var powerUpBallSpawnedNetEvents))
            {
                for (int i = powerUpBallSpawnedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (powerUpBallSpawnedNetEvents[i].OccuredOnTick < tick)
                    {
                        powerUpBallSpawnedNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PowerUpBallObtainedNetEventsPerClient.TryGetValue(clientId, out var powerUpBallObtainedNetEvents))
            {
                for (int i = powerUpBallObtainedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (powerUpBallObtainedNetEvents[i].OccuredOnTick < tick)
                    {
                        powerUpBallObtainedNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (StartMatchCountdownNetEventsPerClient.TryGetValue(clientId, out var startMatchCountdownNetEvents))
            {
                for (int i = startMatchCountdownNetEvents.Count - 1; i >= 0; i--)
                {
                    if (startMatchCountdownNetEvents[i].OccuredOnTick < tick)
                    {
                        startMatchCountdownNetEvents.RemoveAt(i);
                    }
                }
            }

            if (StopMatchCountdownNetEventsPerClient.TryGetValue(clientId, out var stopMatchCountdownNetEvents))
            {
                for (int i = stopMatchCountdownNetEvents.Count - 1; i >= 0; i--)
                {
                    if (stopMatchCountdownNetEvents[i].OccuredOnTick < tick)
                    {
                        stopMatchCountdownNetEvents.RemoveAt(i);
                    }
                }
            }

            if (StageEndNetEventsPerClient.TryGetValue(clientId, out var stageEndNetEvents))
            {
                for (int i = stageEndNetEvents.Count - 1; i >= 0; i--)
                {
                    if (stageEndNetEvents[i].OccuredOnTick < tick)
                    {
                        stageEndNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PlayerLockOnTargetsChangedNetEventsPerClient.TryGetValue(clientId, out var playerLockOnTargetsChangedNetEvents))
            {
                for (int i = playerLockOnTargetsChangedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerLockOnTargetsChangedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerLockOnTargetsChangedNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (PlayerLockedOnTargetHitNetEventsPerClient.TryGetValue(clientId, out var playerLockedOnTargetHitNetEvents))
            {
                for (int i = playerLockedOnTargetHitNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerLockedOnTargetHitNetEvents[i].OccuredOnTick < tick)
                    {
                        playerLockedOnTargetHitNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerPowerUpChangedNetEventsPerClient.TryGetValue(clientId, out var playerPowerUpChangedNetEvents))
            {
                for (int i = playerPowerUpChangedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerPowerUpChangedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerPowerUpChangedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateSonicSlapNetEventsPerClient.TryGetValue(clientId, out var sonicSlapActivatedNetEvents))
            {
                for (int i = sonicSlapActivatedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (sonicSlapActivatedNetEvents[i].OccuredOnTick < tick)
                    {
                        sonicSlapActivatedNetEvents[i].AffectedPlayerIds.Clear();
                        sonicSlapActivatedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (TeamLostNetEventsPerClient.TryGetValue(clientId, out var teamLostNetEvents))
            {
                for (int i = teamLostNetEvents.Count - 1; i >= 0; i--)
                {
                    if (teamLostNetEvents[i].OccuredOnTick < tick)
                    {
                        teamLostNetEvents.RemoveAt(i);
                    }
                }
            }

            if (TalentSwitchNetEventsPerClient.TryGetValue(clientId, out var talentSwitchNetEvents))
            {
                for (int i = talentSwitchNetEvents.Count - 1; i >= 0; i--)
                {
                    if (talentSwitchNetEvents[i].OccuredOnTick < tick)
                    {
                        talentSwitchNetEvents.RemoveAt(i);
                    }
                }
            }

            if (StartMatchEligibleChangedNetEventsPerClient.TryGetValue(clientId, out var startMatchEligibleChangedNetEvents))
            {
                for (int i = startMatchEligibleChangedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (startMatchEligibleChangedNetEvents[i].OccuredOnTick < tick)
                    {
                        startMatchEligibleChangedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (EnvironmentSpringPlayerCollisionNetEventsPerClient.TryGetValue(clientId, out var environmentSpringPlayerCollisionNetEvents))
            {
                for (int i = environmentSpringPlayerCollisionNetEvents.Count - 1; i >= 0; i--)
                {
                    if (environmentSpringPlayerCollisionNetEvents[i].OccuredOnTick < tick)
                    {
                        environmentSpringPlayerCollisionNetEvents.RemoveAt(i);
                    }
                }
            }

            if (GainBoltsNetEventsPerClient.TryGetValue(clientId, out var gainBoltsNetEvents))
            {
                for (int i = gainBoltsNetEvents.Count - 1; i >= 0; i--)
                {
                    if (gainBoltsNetEvents[i].OccuredOnTick < tick)
                    {
                        gainBoltsNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (EnvironmentSpikePlayerCollisionNetEventsPerClient.TryGetValue(clientId, out var environmentSpikePlayerCollisionNetEvents))
            {
                for (int i = environmentSpikePlayerCollisionNetEvents.Count - 1; i >= 0; i--)
                {
                    if (environmentSpikePlayerCollisionNetEvents[i].OccuredOnTick <= tick)
                    {
                        environmentSpikePlayerCollisionNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient.TryGetValue(clientId, out var playerToEnvironmentTeleportGateCollisionNetEvents))
            {
                for (int i = playerToEnvironmentTeleportGateCollisionNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerToEnvironmentTeleportGateCollisionNetEvents[i].OccuredOnTick < tick)
                    {
                        playerToEnvironmentTeleportGateCollisionNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PreparationPhaseEndedNetEventsPerClient.TryGetValue(clientId, out var preparationPhaseEndedNetEvents))
            {
                for (int i = preparationPhaseEndedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (preparationPhaseEndedNetEvents[i].OccuredOnTick < tick)
                    {
                        preparationPhaseEndedNetEvents.RemoveAt(i);
                    }
                }
            }
            if (CreateSwapFieldNetEventsPerClient.TryGetValue(clientId, out var createSwapFieldNetEvents))
            {
                for (int i = createSwapFieldNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createSwapFieldNetEvents[i].OccuredOnTick < tick)
                    {
                        createSwapFieldNetEvents.RemoveAt(i);
                    }
                }
            }
            if (DeactivateSwapTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateSwapTalentNetEvents))
            {
                for (int i = deactivateSwapTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateSwapTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateSwapTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (CreateKOProjectileNetEventsPerClient.TryGetValue(clientId, out var createKOProjectileNetEvents))
            {
                for (int i = createKOProjectileNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createKOProjectileNetEvents[i].OccuredOnTick < tick)
                    {
                        createKOProjectileNetEvents.RemoveAt(i);
                    }
                }
            }
            if (KOProjectHitPlayerNetEventsPerClient.TryGetValue(clientId, out var koProjectHitPlayerNetEvents))
            {
                for (int i = koProjectHitPlayerNetEvents.Count - 1; i >= 0; i--)
                {
                    if (koProjectHitPlayerNetEvents[i].OccuredOnTick < tick)
                    {
                        koProjectHitPlayerNetEvents.RemoveAt(i);
                    }
                }
            }

if (DeactivateKOTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateKOTalentNetEvents))
            {
                for (int i = deactivateKOTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateKOTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateKOTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (PerformDashPulseNetEventsPerClient.TryGetValue(clientId, out var performDashPulseNetEvents))
            {
                for (int i = performDashPulseNetEvents.Count - 1; i >= 0; i--)
                {
                    if (performDashPulseNetEvents[i].OccuredOnTick < tick)
                    {
                        performDashPulseNetEvents.RemoveAt(i);
                    }
                }
            }
            if (ActivateSentryGunTalentNetEventsPerClient.TryGetValue(clientId, out var activateSentryGunTalentNetEvents))
            {
                for (int i = activateSentryGunTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateSentryGunTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        activateSentryGunTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (DeactivateSentryGunTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateSentryGunTalentNetEvents))
            {
                for (int i = deactivateSentryGunTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateSentryGunTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateSentryGunTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (UpdatePlayerTalentStocksNetEventsPerClient.TryGetValue(clientId, out var updatePlayerTalentsStocksNetEvnets))
            {
                for (int i = updatePlayerTalentsStocksNetEvnets.Count - 1; i >= 0; i--)
                {
                    if (updatePlayerTalentsStocksNetEvnets[i].OccuredOnTick < tick)
                    {
                        updatePlayerTalentsStocksNetEvnets.RemoveAt(i);
                    }
                }
            }
            if (PlayerMaxShootCooldownChangedNetEventsPerClient.TryGetValue(clientId, out var playerMaxShootCooldownChangedNetEvents))
            {
                for (int i = playerMaxShootCooldownChangedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (playerMaxShootCooldownChangedNetEvents[i].OccuredOnTick < tick)
                    {
                        playerMaxShootCooldownChangedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (CreateGrapplingHookProjectileNetEventsPerClient.TryGetValue(clientId, out var createGrapplingHookProjectileNetEvents))
            {
                for (int i = createGrapplingHookProjectileNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createGrapplingHookProjectileNetEvents[i].OccuredOnTick < tick)
                    {
                        createGrapplingHookProjectileNetEvents.RemoveAt(i);
                    }
                }
            }

            if (GrapplingHookHitWallNetEventsPerClient.TryGetValue(clientId, out var grapplingHookHitWallNetEvents))
            {
                for (int i = grapplingHookHitWallNetEvents.Count - 1; i >= 0; i--)
                {
                    if (grapplingHookHitWallNetEvents[i].OccuredOnTick < tick)
                    {
                        grapplingHookHitWallNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateGrapplingHookTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateGrapplingHookTalentNetEvents))
            {
                for (int i = deactivateGrapplingHookTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateGrapplingHookTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateGrapplingHookTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (CreateFishingRodProjectileNetEventsPerClient.TryGetValue(clientId, out var createFishingRodProjectileNetEvents))
            {
                for (int i = createFishingRodProjectileNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createFishingRodProjectileNetEvents[i].OccuredOnTick < tick)
                    {
                        createFishingRodProjectileNetEvents.RemoveAt(i);
                    }
                }
            }

            if (FishingRodCaughtEnemyNetEventsPerClient.TryGetValue(clientId, out var fishingRodCaughtEnemyNetEvents))
            {
                for (int i = fishingRodCaughtEnemyNetEvents.Count - 1; i >= 0; i--)
                {
                    if (fishingRodCaughtEnemyNetEvents[i].OccuredOnTick < tick)
                    {
                        fishingRodCaughtEnemyNetEvents.RemoveAt(i);
                    }
                }
            }

            if (FishingRodTipHitWallNetEventsPerClient.TryGetValue(clientId, out var fishingRodTipHitWallNetEvents))
            {
                for (int i = fishingRodTipHitWallNetEvents.Count - 1; i >= 0; i--)
                {
                    if (fishingRodTipHitWallNetEvents[i].OccuredOnTick < tick)
                    {
                        fishingRodTipHitWallNetEvents.RemoveAt(i);
                    }
                }
            }

            if (FishingRodThrowNetEventsPerClient.TryGetValue(clientId, out var fishingRodThrowNetEvents))
            {
                for (int i = fishingRodThrowNetEvents.Count - 1; i >= 0; i--)
                {
                    if (fishingRodThrowNetEvents[i].OccuredOnTick < tick)
                    {
                        fishingRodThrowNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateFishingRodTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateFishingRodTalentNetEvents))
            {
                for (int i = deactivateFishingRodTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateFishingRodTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateFishingRodTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (CreateSoulGhostNetEventsPerClient.TryGetValue(clientId, out var createSoulGhostNetEvents))
            {
                for (int i = createSoulGhostNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createSoulGhostNetEvents[i].OccuredOnTick < tick)
                    {
                        createSoulGhostNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateSoulTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateSoulTalentNetEvents))
            {
                for (int i = deactivateSoulTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateSoulTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateSoulTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateRockTalentNetEventsPerClient.TryGetValue(clientId, out var activateRockTalentNetEvents))
            {
                for (int i = activateRockTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateRockTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        activateRockTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateRockTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateRockTalentNetEvents))
            {
                for (int i = deactivateRockTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateRockTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateRockTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateFrozenTalentNetEventsPerClient.TryGetValue(clientId, out var activateFrozenTalentNetEvents))
            {
                for (int i = activateFrozenTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateFrozenTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        activateFrozenTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (MoleSpawnedNetEventsPerClient.TryGetValue(clientId, out var moleSpawnedNetEvents))
            {
                for (int i = moleSpawnedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (moleSpawnedNetEvents[i].OccuredOnTick < tick)
                    {
                        moleSpawnedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (MoleHitNetEventsPerClient.TryGetValue(clientId, out var moleHitNetEvents))
            {
                for (int i = moleHitNetEvents.Count - 1; i >= 0; i--)
                {
                    if (moleHitNetEvents[i].OccuredOnTick < tick)
                    {
                        moleHitNetEvents.RemoveAt(i);
                    }
                }
            }

            if (MoleExpiredNetEventsPerClient.TryGetValue(clientId, out var moleExpiredNetEvents))
            {
                for (int i = moleExpiredNetEvents.Count - 1; i >= 0; i--)
                {
                    if (moleExpiredNetEvents[i].OccuredOnTick < tick)
                    {
                        moleExpiredNetEvents.RemoveAt(i);
                    }
                }
            }

            if (GoldenMoleDamagedNetEventsPerClient.TryGetValue(clientId, out var goldenMoleDamagedNetEvents))
            {
                for (int i = goldenMoleDamagedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (goldenMoleDamagedNetEvents[i].OccuredOnTick < tick)
                    {
                        goldenMoleDamagedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ScoreGatePassedNetEventsPerClient.TryGetValue(clientId, out var scoreGatePassedNetEvents))
            {
                for (int i = scoreGatePassedNetEvents.Count - 1; i >= 0; i--)
                {
                    if (scoreGatePassedNetEvents[i].OccuredOnTick < tick)
                    {
                        scoreGatePassedNetEvents.RemoveAt(i);
                    }
                }
            }

            if (GateTrapClosingNetEventsPerClient.TryGetValue(clientId, out var gateTrapClosingNetEvents))
            {
                for (int i = gateTrapClosingNetEvents.Count - 1; i >= 0; i--)
                {
                    if (gateTrapClosingNetEvents[i].OccuredOnTick < tick)
                    {
                        gateTrapClosingNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateFrozenTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateFrozenTalentNetEvents))
            {
                for (int i = deactivateFrozenTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateFrozenTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateFrozenTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (CreateMagneticPullFieldNetEventsPerClient.TryGetValue(clientId, out var createMagneticPullFieldNetEvents))
            {
                for (int i = createMagneticPullFieldNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createMagneticPullFieldNetEvents[i].OccuredOnTick < tick)
                    {
                        createMagneticPullFieldNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateUmbrellaTalentNetEventsPerClient.TryGetValue(clientId, out var activateUmbrellaTalentNetEvents))
            {
                for (int i = activateUmbrellaTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateUmbrellaTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        activateUmbrellaTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateUmbrellaTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateUmbrellaTalentNetEvents))
            {
                for (int i = deactivateUmbrellaTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateUmbrellaTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateUmbrellaTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateWaterGunTalentNetEventsPerClient.TryGetValue(clientId, out var activateWaterGunTalentNetEvents))
            {
                for (int i = activateWaterGunTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateWaterGunTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        activateWaterGunTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateWaterGunTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateWaterGunTalentNetEvents))
            {
                for (int i = deactivateWaterGunTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateWaterGunTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateWaterGunTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateHeadbuttChargingNetEventsPerClient.TryGetValue(clientId, out var activateHeadbuttChargingNetEvents))
            {
                for (int i = activateHeadbuttChargingNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateHeadbuttChargingNetEvents[i].OccuredOnTick < tick)
                    {
                        activateHeadbuttChargingNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PerformBarrelDashNetEventsPerClient.TryGetValue(clientId, out var performBarrelDashNetEvents))
            {
                for (int i = performBarrelDashNetEvents.Count - 1; i >= 0; i--)
                {
                    if (performBarrelDashNetEvents[i].OccuredOnTick < tick)
                    {
                        performBarrelDashNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PerformHeadbuttDashNetEventsPerClient.TryGetValue(clientId, out var performHeadbuttDashNetEvents))
            {
                for (int i = performHeadbuttDashNetEvents.Count - 1; i >= 0; i--)
                {
                    if (performHeadbuttDashNetEvents[i].OccuredOnTick < tick)
                    {
                        performHeadbuttDashNetEvents.RemoveAt(i);
                    }
                }
            }

            if (HeadbuttHitEnemyNetEventsPerClient.TryGetValue(clientId, out var headbuttHitEnemyNetEvents))
            {
                for (int i = headbuttHitEnemyNetEvents.Count - 1; i >= 0; i--)
                {
                    if (headbuttHitEnemyNetEvents[i].OccuredOnTick < tick)
                    {
                        headbuttHitEnemyNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateHeadbuttTalentNetEventsPerClient.TryGetValue(clientId, out var deactivateHeadbuttTalentNetEvents))
            {
                for (int i = deactivateHeadbuttTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateHeadbuttTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateHeadbuttTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (LayChickenEggNetEventsPerClient.TryGetValue(clientId, out var layChickenEggNetEvents))
            {
                for (int i = layChickenEggNetEvents.Count - 1; i >= 0; i--)
                {
                    if (layChickenEggNetEvents[i].OccuredOnTick < tick)
                    {
                        layChickenEggNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ChickenEggHitNetEventsPerClient.TryGetValue(clientId, out var chickenEggHitNetEvents))
            {
                for (int i = chickenEggHitNetEvents.Count - 1; i >= 0; i--)
                {
                    if (chickenEggHitNetEvents[i].OccuredOnTick < tick)
                    {
                        chickenEggHitNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateYearsOfPainTalentNetEventsPerClient.TryGetValue(clientId, out var activateYearsOfPainTalentNetEvents))
            {
                for (int i = activateYearsOfPainTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateYearsOfPainTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        activateYearsOfPainTalentNetEvents.RemoveAt(i);
                    }
                }
            }

            if (PerformGalacticPullNetEventsPerClient.TryGetValue(clientId, out var performGalacticPullNetEvents))
            {
                for (int i = performGalacticPullNetEvents.Count - 1; i >= 0; i--)
                {
                    if (performGalacticPullNetEvents[i].OccuredOnTick < tick)
                    {
                        performGalacticPullNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DeactivateGalacticForceFieldNetEventsPerClient.TryGetValue(clientId, out var deactivateGalacticForceFieldNetEvents))
            {
                for (int i = deactivateGalacticForceFieldNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateGalacticForceFieldNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateGalacticForceFieldNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (ActivateNukePowerUpNetEventsPerClient.TryGetValue(clientId, out var activateNukePowerUpNetEvents))
            {
                for (int i = activateNukePowerUpNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateNukePowerUpNetEvents[i].OccuredOnTick < tick)
                    {
                        activateNukePowerUpNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (DeactivateShufflePowerUpNetEventsPerClient.TryGetValue(clientId, out var activateShufflePowerUpNetEvents))
            {
                for (int i = activateShufflePowerUpNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateShufflePowerUpNetEvents[i].OccuredOnTick < tick)
                    {
                        activateShufflePowerUpNetEvents.RemoveAt(i);
                    }
                }
            }
            
            if (ShuffleSwapPlayerPositionNetEventsPerClient.TryGetValue(clientId, out var shuffleSwapPlayerPositionNetEvents))
            {
                for (int i = shuffleSwapPlayerPositionNetEvents.Count - 1; i >= 0; i--)
                {
                    if (shuffleSwapPlayerPositionNetEvents[i].OccuredOnTick < tick)
                    {
                        shuffleSwapPlayerPositionNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ActivateShuffleNetEventsPerClient.TryGetValue(clientId, out var activateShuffleNetEvents))
            {
                for (int i = activateShuffleNetEvents.Count - 1; i >= 0; i--)
                {
                    if (activateShuffleNetEvents[i].OccuredOnTick < tick)
                    {
                        activateShuffleNetEvents.RemoveAt(i);
                    }
                }
            }

            if (StartPowerUpGrantingPhaseNetEventsPerClient.TryGetValue(clientId, out var startPowerUpGrantingPhaseNetEvents))
            {
                for (int i = startPowerUpGrantingPhaseNetEvents.Count - 1; i >= 0; i--)
                {
                    if (startPowerUpGrantingPhaseNetEvents[i].OccuredOnTick < tick)
                    {
                        startPowerUpGrantingPhaseNetEvents.RemoveAt(i);
                    }
                }
            }

            if (EndPowerUpGrantingPhaseNetEventsPerClient.TryGetValue(clientId, out var endPowerUpGrantingPhaseNetEvents))
            {
                for (int i = endPowerUpGrantingPhaseNetEvents.Count - 1; i >= 0; i--)
                {
                    if (endPowerUpGrantingPhaseNetEvents[i].OccuredOnTick < tick)
                    {
                        endPowerUpGrantingPhaseNetEvents.RemoveAt(i);
                    }
                }
            }

            if (ShootFrigidBlockNetEventsPerClient.TryGetValue(clientId, out var shootFrigidBlockNetEvents))
            {
                for (int i = shootFrigidBlockNetEvents.Count - 1; i >= 0; i--)
                {
                    if (shootFrigidBlockNetEvents[i].OccuredOnTick < tick)
                    {
                        shootFrigidBlockNetEvents.RemoveAt(i);
                    }
                }
            }

            if (DestroyFrigidBlockNetEventsPerClient.TryGetValue(clientId, out var destroyFrigidBlockNetEvents))
            {
                for (int i = destroyFrigidBlockNetEvents.Count - 1; i >= 0; i--)
                {
                    if (destroyFrigidBlockNetEvents[i].OccuredOnTick < tick)
                    {
                        destroyFrigidBlockNetEvents.RemoveAt(i);
                    }
                }
            }
        }

        public void AddStartMatchCountdownNetEvent(int onTick, ushort seconds)
        {
            foreach (var kvp in StartMatchCountdownNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CountdownSeconds = seconds;
            }
        }

        public void AddStopMatchCountdownNetEvent(int onTick)
        {
            foreach (var kvp in StopMatchCountdownNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
            }
        }

        public void AddStartMatchEligibleChangedNetEvent(int onTick, bool isEligible)
        {
            foreach (var kvp in StartMatchEligibleChangedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.IsEligible = isEligible;
            }
        }

        public void AddStageEndNetEvent(int onTick, ushort winningTeamId, Dictionary<ushort, int> jemsWon, Dictionary<ushort, int> totalJems, ushort playerIdToFocusOn)
        {
            foreach (var kvp in StageEndNetEventsPerClient)
            {
                var packet = kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.WinningTeamId = winningTeamId;
                packet.PlayerIdToFocusOn = playerIdToFocusOn;
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
            foreach (var kvp in TeamLostNetEventsPerClient)
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
            foreach (var kvp in TalentSwitchNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
                packet.NewTalentIndex = newTalentIndex;
            }
        }

        public void AddPlayerSpinnedStartedNetEvent(int onTick, ushort playerId)
        {
            foreach (var kvp in PlayerSpinnedStartedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
            }
        }

        public void AddPlayerSpinnedEndedNetEvent(int onTick, ushort playerId)
        {
            foreach (var kvp in PlayerSpinnedEndedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
            }
        }

        public void AddPlayerStartedExposedToLavaNetEvent(int onTick, ushort playerId)
        {
            foreach (var kvp in PlayerStartedExposedToLavaNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
            }
        }

        public void AddPlayerEndedExposedToLavaNetEvent(int onTick, ushort playerId)
        {
            foreach (var kvp in PlayerEndedExposedToLavaNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.PlayerId = playerId;
            }
        }

        public void AddEnvironmentSpringPlayerCollisionNetEvent(int onTick, ushort springId, ushort playerId, Vector2 newPlayerDirection)
        {
            foreach (var kvp in EnvironmentSpringPlayerCollisionNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.SpringId = springId;
                packet.PlayerId = playerId;
                packet.NewPlayerDirection = newPlayerDirection;
            }
        }

        public void AddEnvironmentSpikePlayerCollisionNetEvent(int onTick, ushort spikeId, ushort playerId)
        {
            foreach (var kvp in EnvironmentSpikePlayerCollisionNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.SpikeId = spikeId;
                packet.PlayerId = playerId;
            }
        }

        public void AddGainBoltsNetEvent(int onTick, ushort playerId, int gainedAmount, int totalTeamBolts)
        {
            foreach (var kvp in GainBoltsNetEventsPerClient)
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
            foreach (var kvp in PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient)
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
            foreach (var kvp in PreparationPhaseEndedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
            }
        }

        public void AddCreateSwapFieldNetEvent(int onTick, ushort swapFieldId, ushort casterPlayerId, int fieldEndTick, float maxRadius)
        {
            foreach (var kvp in CreateSwapFieldNetEventsPerClient)
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
            foreach (var kvp in DeactivateSwapTalentNetEventsPerClient)
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
            foreach (var kvp in CreateKOProjectileNetEventsPerClient)
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
            foreach (var kvp in KOProjectHitPlayerNetEventsPerClient)
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
            foreach (var kvp in DeactivateKOTalentNetEventsPerClient)
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
            foreach (var kvp in PerformDashPulseNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddUpdatePlayerTalentStocksNetEventS2C(int onTick, ushort casterPlayerId, TalentType talentType, int currentStocksAmount, int recieveNextStockOnTick)
        {
            foreach (var kvp in UpdatePlayerTalentStocksNetEventsPerClient)
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
            foreach (var kvp in ActivateSentryGunTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddDeactivateSentryGunTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateSentryGunTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddCreateGrapplingHookProjectileNetEvent(int onTick, ushort projectileId, ushort playerCasterId, Vector2 position)
        {
            foreach (var kvp in CreateGrapplingHookProjectileNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.GrapplingHookProjectile.Id = projectileId;
                packet.GrapplingHookProjectile.PlayerCasterId = playerCasterId;
                packet.GrapplingHookProjectile.Position = position;
            }
        }

        public void AddGrapplingHookHitWallNetEvent(int onTick, ushort projectileId, ushort hitWallId, System.Numerics.Vector2 hitPosition)
        {
            foreach (var kvp in GrapplingHookHitWallNetEventsPerClient)
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
            foreach (var kvp in DeactivateGrapplingHookTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.ProjectileId = projectileId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddCreateFishingRodProjectileNetEvent(int onTick, ushort projectileId, ushort playerCasterId, Vector2 position)
        {
            foreach (var kvp in CreateFishingRodProjectileNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.FishingRodProjectile.Id = projectileId;
                packet.FishingRodProjectile.PlayerCasterId = playerCasterId;
                packet.FishingRodProjectile.Position = position;
                packet.FishingRodProjectile.Phase = FishingRodTipPhase.FlyingForward;
                packet.FishingRodProjectile.CaughtEnemyId = 0;
                packet.FishingRodProjectile.CaughtEnemyType = FishingRodCaughtEnemyType.None;
            }
        }

        public void AddFishingRodCaughtEnemyNetEvent(int onTick, ushort projectileId, ushort casterPlayerId, ushort caughtEnemyId, FishingRodCaughtEnemyType caughtEnemyType)
        {
            foreach (var kvp in FishingRodCaughtEnemyNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.ProjectileId = projectileId;
                packet.CasterPlayerId = casterPlayerId;
                packet.CaughtEnemyId = caughtEnemyId;
                packet.CaughtEnemyType = caughtEnemyType;
            }
        }

        public void AddFishingRodTipHitWallNetEvent(int onTick, ushort projectileId, Vector2 hitPosition)
        {
            foreach (var kvp in FishingRodTipHitWallNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.ProjectileId = projectileId;
                packet.HitPosition = hitPosition;
            }
        }

        public void AddFishingRodThrowNetEvent(int onTick, ushort casterPlayerId, ushort thrownEnemyId, FishingRodCaughtEnemyType thrownEnemyType, Vector2 throwDirection)
        {
            foreach (var kvp in FishingRodThrowNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.ThrownEnemyId = thrownEnemyId;
                packet.ThrownEnemyType = thrownEnemyType;
                packet.ThrowDirection = throwDirection;
            }
        }

        public void AddDeactivateFishingRodTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateFishingRodTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.ProjectileId = projectileId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddCreateSoulGhostNetEvent(int onTick, ushort ghostId, ushort playerCasterId, Vector2 position, Vector2 direction)
        {
            foreach (var kvp in CreateSoulGhostNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.SoulGhost.Id = ghostId;
                packet.SoulGhost.PlayerCasterId = playerCasterId;
                packet.SoulGhost.Position = position;
                packet.SoulGhost.Direction = direction;
                packet.SoulGhost.Velocity = Vector2.Zero;
            }
        }

        public void AddDeactivateSoulTalentNetEvent(int onTick, ushort ghostId, ushort casterPlayerId, int talentCooldownEndTick, bool didTeleport, Vector2 teleportPosition,
            Vector2 teleportDirection)
        {
            foreach (var kvp in DeactivateSoulTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.GhostId = ghostId;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
                packet.DidTeleport = didTeleport;
                packet.TeleportPosition = teleportPosition;
                packet.TeleportDirection = teleportDirection;
            }
        }

        public void AddActivateRockTalentNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in ActivateRockTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddDeactivateRockTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateRockTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddActivateFrozenTalentNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in ActivateFrozenTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddDeactivateFrozenTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateFrozenTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddMoleSpawnedNetEvent(int onTick, ushort moleId, System.Numerics.Vector2 position, int emergeOnTick, bool isGolden, byte maxLives)
        {
            foreach (var kvp in MoleSpawnedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.MoleId = moleId;
                packet.Position = position;
                packet.EmergeOnTick = emergeOnTick;
                packet.IsGolden = isGolden;
                packet.MaxLives = maxLives;
            }
        }

        public void AddMoleHitNetEvent(int onTick, ushort moleId, ushort byPlayerId, ushort byTeamId, byte scoreGained, int teamMolesHitTotal, int byPlayerMolesHitScoreTotal, bool isGolden)
        {
            foreach (var kvp in MoleHitNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.MoleId = moleId;
                packet.ByPlayerId = byPlayerId;
                packet.ByTeamId = byTeamId;
                packet.ScoreGained = scoreGained;
                packet.TeamMolesHitTotal = teamMolesHitTotal;
                packet.ByPlayerMolesHitScoreTotal = byPlayerMolesHitScoreTotal;
                packet.IsGolden = isGolden;
            }
        }

        public void AddScoreGatePassedNetEvent(int onTick, ushort scoreGateId, ushort byPlayerId, ushort byTeamId, byte scoreGained, byte newScoreMultiplier, int teamBonusScoreTotal, int byPlayerBonusScoreTotal)
        {
            foreach (var kvp in ScoreGatePassedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.ScoreGateId = scoreGateId;
                packet.ByPlayerId = byPlayerId;
                packet.ByTeamId = byTeamId;
                packet.ScoreGained = scoreGained;
                packet.NewScoreMultiplier = newScoreMultiplier;
                packet.TeamBonusScoreTotal = teamBonusScoreTotal;
                packet.ByPlayerBonusScoreTotal = byPlayerBonusScoreTotal;
            }
        }

        public void AddGateTrapClosingNetEvent(int onTick, ushort gateTrapId, int closedOnTick)
        {
            foreach (var kvp in GateTrapClosingNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.GateTrapId = gateTrapId;
                packet.ClosedOnTick = closedOnTick;
            }
        }

        public void AddMoleExpiredNetEvent(int onTick, ushort moleId, int hideOnTick)
        {
            foreach (var kvp in MoleExpiredNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.MoleId = moleId;
                packet.HideOnTick = hideOnTick;
            }
        }

        public void AddGoldenMoleDamagedNetEvent(int onTick, ushort moleId, byte remainingLives, byte maxLives)
        {
            foreach (var kvp in GoldenMoleDamagedNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.MoleId = moleId;
                packet.RemainingLives = remainingLives;
                packet.MaxLives = maxLives;
            }
        }

        public void AddActivateUmbrellaTalentNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in ActivateUmbrellaTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddDeactivateUmbrellaTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateUmbrellaTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddActivateWaterGunTalentNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in ActivateWaterGunTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddDeactivateWaterGunTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateWaterGunTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddActivateHeadbuttChargingNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in ActivateHeadbuttChargingNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddPerformHeadbuttDashNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in PerformHeadbuttDashNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddPerformBarrelDashNetEvent(int onTick, ushort casterPlayerId)
        {
            foreach (var kvp in PerformBarrelDashNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
            }
        }

        public void AddHeadbuttHitEnemyNetEvent(int onTick, ushort casterPlayerId, ushort enemyPlayerId)
        {
            foreach (var kvp in HeadbuttHitEnemyNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.EnemyPlayerId = enemyPlayerId;
            }
        }

        public void AddDeactivateHeadbuttTalentNetEvent(int onTick, ushort casterPlayerId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateHeadbuttTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddCreateMagneticPullFieldNetEventS2C(int onTick, ushort casterPlayerId, Vector2 position, Vector2 direction, int talentCooldownEndTick, bool hasHit, ushort hitEnemyId)
        {
            foreach (var kvp in CreateMagneticPullFieldNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.Position = position;
                packet.Direction = direction;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
                packet.HasHit = hasHit;
                packet.HitEnemyId = hitEnemyId;
            }
        }

        public void AddLayChickenEggNetEventS2C(int tick, ushort casterId, ushort eggId, Vector2 position)
        {
            foreach (var kvp in LayChickenEggNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = tick;
                packet.CasterPlayerId = casterId;
                packet.EggId = eggId;
                packet.Position = position;
            }
        }

        public void AddActivateYearsOfPainTalentNetEventS2C(int onTick, ushort casterPlayerId, Vector2 direction, int talentCooldownEndTick, bool hasHit, ushort hitEnemyId)
        {
            foreach (var kvp in ActivateYearsOfPainTalentNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.Direction = direction;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
                packet.HasHit = hasHit;
                packet.HitEnemyId = hitEnemyId;
            }
        }

        public void AddChickenEggHitNetEventS2C(int tick, ushort eggId)
        {
            foreach (var kvp in ChickenEggHitNetEventsPerClient)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = tick;
                packet.EggId = eggId;
            }
        }
    }
}