using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.LocalEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using LiteNetLib.Utils;
using UnityEngine;
using Core.Game.Domains.GamePlay.Presentation.Scripts.DataService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers
{
    public class MatchFullTickPacketsHandler : IFullTickPacketsHandler, IGUIUpdatable
    {
        private readonly NetworkConfig _networkConfig;
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ILastFullSyncTickDataService _lastFullSyncTickDataService;
        private readonly IInterpolationDecayService _interpolationDecayService;
        private readonly INetworkDiagnosticsService _networkDiagnosticsService;

        private readonly PresentationMatchNetEventsHandler _presentationNetEventsHandler;
        private readonly CapacityDict<int, MatchFullTickPacketS2C> _fullTickPacketsUnprocessedByLogic;
        private readonly CapacityDict<int, MatchFullTickPacketS2C> _fullTickPacketsUnprocessedByView;
        private readonly CapacityList<PlayerRejoinAcceptPacketS2C> _cachedUnprocessedPlayerRejoinedEvents;
        private readonly CapacityList<BulletSpawnNetEventS2C> _cachedUnprocessedBulletSpawnedEvents;
        private readonly CapacityList<PlayerTakeDamageNetEventS2C> _cachedUnprocessedPlayerTakeDamageEvents;
        private readonly CapacityList<PlayerDiedNetEventS2C> _cachedUnprocessedPlayerDiedEvents;
        private readonly CapacityList<BulletDestroyedNetEventS2C> _cachedUnprocessedBulletDestroyedEvents;
        private readonly CapacityList<PlayersSwapNetEventS2C> _cachedUnprocessedPlayerSwapEvents;
        private readonly CapacityList<TalentCardObtainedNetEventS2C> _cachedUnprocessedTalentCardObtainedEvents;
        private readonly CapacityList<TalentCardHitNetEventS2C> _cachedUnprocessedTalentCardHitEvents;
        private readonly CapacityList<PlayerSpinnedStartedNetEventS2C> _cachedUnprocessedPlayerSpinnedStartedEvents;
        private readonly CapacityList<PlayerSpinnedEndedNetEventS2C> _cachedUnprocessedPlayerSpinnedEndedEvents;
        private readonly CapacityList<PlayerStartedExposedToLavaNetEventS2C> _cachedUnprocessedPlayerStartedExposedToLavaEvents;
        private readonly CapacityList<PlayerEndedExposedToLavaNetEventS2C> _cachedUnprocessedPlayerEndedExposedToLavaEvents;
        private readonly CapacityList<PowerUpBallSpawnedNetEventS2C> _cachedUnprocessedPowerUpBallSpawnedEvents;
        private readonly CapacityList<PowerUpBallObtainedNetEventS2C> _cachedUnprocessedPowerUpBallObtainedEvents;
        private readonly CapacityList<MoleSpawnedNetEventS2C> _cachedUnprocessedMoleSpawnedEvents;
        private readonly CapacityList<MoleKilledNetEventS2C> _cachedUnprocessedMoleKilledEvents;
        private readonly CapacityList<MoleExpiredNetEventS2C> _cachedUnprocessedMoleExpiredEvents;
        private readonly CapacityList<GoldenMoleDamagedNetEventS2C> _cachedUnprocessedGoldenMoleDamagedEvents;
        private readonly CapacityList<PlayerPassedScoreGateNetEventS2C> _cachedUnprocessedPlayerPassedScoreGateEvents;
        private readonly CapacityList<GateTrapClosingNetEventS2C> _cachedUnprocessedGateTrapClosingEvents;
        private readonly CapacityList<StageEndNetEventS2C> _cachedUnprocessedStageEndEvents;
        private readonly CapacityList<TeamLostNetEventS2C> _cachedUnprocessedTeamLostEvents;
        private readonly CapacityList<TalentSwitchNetEventS2C> _cachedUnprocessedTalentSwitchEvents;
        private readonly CapacityList<GainBoltsNetEventS2C> _cachedUnprocessedGainBoltsEvents;
        private readonly CapacityList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents;
        private readonly CapacityList<EnvironmentSpringPlayerCollisionNetEventS2C> _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents;
        private readonly CapacityList<EnvironmentSpikePlayerCollisionNetEventS2C> _cachedUnprocessedEnvironmentSpikePlayerCollisionEvents;
        private readonly CapacityList<PreparationPhaseEndedNetEventS2C> _cachedUnprocessedPreparationPhaseEndedEvents;
        private readonly CapacityList<CreateSwapFieldNetEventS2C> _cachedUnprocessedCreateSwapFieldEvents;
        private readonly CapacityList<DeactivateSwapTalentNetEventS2C> _cachedUnprocessedDeactivateSwapTalentEvents;
        private readonly CapacityList<KOProjectHitPlayerNetEventS2C> _cachedUnprocessedKOProjectHitPlayerEvents;
        private readonly CapacityList<CreateKOProjectileNetEventS2C> _cachedUnprocessedCreateKOProjectileEvents;
        private readonly CapacityList<DeactivateKOTalentNetEventS2C> _cachedUnprocessedDeactivateKOTalentEvents;
        private readonly CapacityList<CreateGrapplingHookProjectileNetEventS2C> _cachedUnprocessedCreateGrapplingHookProjectileEvents;
        private readonly CapacityList<GrapplingHookHitWallNetEventS2C> _cachedUnprocessedGrapplingHookHitWallEvents;
        private readonly CapacityList<DeactivateGrapplingHookTalentNetEventS2C> _cachedUnprocessedDeactivateGrapplingHookTalentEvents;
        private readonly CapacityList<CreateFishingRodProjectileNetEventS2C> _cachedUnprocessedCreateFishingRodProjectileEvents;
        private readonly CapacityList<FishingRodCaughtEnemyNetEventS2C> _cachedUnprocessedFishingRodCaughtEnemyEvents;
        private readonly CapacityList<FishingRodTipHitWallNetEventS2C> _cachedUnprocessedFishingRodTipHitWallEvents;
        private readonly CapacityList<FishingRodThrowNetEventS2C> _cachedUnprocessedFishingRodThrowEvents;
        private readonly CapacityList<DeactivateFishingRodTalentNetEventS2C> _cachedUnprocessedDeactivateFishingRodTalentEvents;
        private readonly CapacityList<CreateSoulGhostNetEventS2C> _cachedUnprocessedCreateSoulGhostEvents;
        private readonly CapacityList<DeactivateSoulTalentNetEventS2C> _cachedUnprocessedDeactivateSoulTalentEvents;
        private readonly CapacityList<ActivateRockTalentNetEventS2C> _cachedUnprocessedActivateRockTalentEvents;
        private readonly CapacityList<DeactivateRockTalentNetEventS2C> _cachedUnprocessedDeactivateRockTalentEvents;
        private readonly CapacityList<ActivateFrozenTalentNetEventS2C> _cachedUnprocessedActivateFrozenTalentEvents;
        private readonly CapacityList<DeactivateFrozenTalentNetEventS2C> _cachedUnprocessedDeactivateFrozenTalentEvents;
        private readonly CapacityList<ShootFrigidBlockNetEventS2C> _cachedUnprocessedShootFrigidBlockEvents;
        private readonly CapacityList<DestroyFrigidBlockNetEventS2C> _cachedUnprocessedDestroyFrigidBlockEvents;
        private readonly CapacityList<ActivateSentryGunTalentNetEventS2C> _cachedUnprocessedActivateSentryGunTalentEvents;
        private readonly CapacityList<DeactivateSentryGunTalentNetEventS2C> _cachedUnprocessedDeactivateSentryGunTalentEvents;
        private readonly CapacityList<PerformDashPulseNetEventS2C> _cachedUnprocessedPerformDashPulseEvents;
        private readonly CapacityList<UpdatePlayerTalentStocksNetEventS2C> _cachedUnprocessedUpdatePlayerTalentStocksEvents;
        private readonly CapacityList<PlayerMaxShootCooldownChangedNetEventS2C> _cachedUnprocessedPlayerMaxShootCooldownChangedEvents;
        private readonly CapacityList<PlayerSelectedTalentFinishedCooldownLocalEvent> _cachedPlayerSelectedTalentFinishedCooldownLocalEvents;
        private readonly CapacityList<ActivateUmbrellaTalentNetEventS2C> _cachedUnprocessedActivateUmbrellaTalentEvents;
        private readonly CapacityList<DeactivateUmbrellaTalentNetEventS2C> _cachedUnprocessedDeactivateUmbrellaTalentEvents;
        private readonly CapacityList<ActivateWaterGunTalentNetEventS2C> _cachedUnprocessedActivateWaterGunTalentEvents;
        private readonly CapacityList<DeactivateWaterGunTalentNetEventS2C> _cachedUnprocessedDeactivateWaterGunTalentEvents;
        private readonly CapacityList<ActivateHeadbuttChargingNetEventS2C> _cachedUnprocessedActivateHeadbuttChargingEvents;
        private readonly CapacityList<PerformHeadbuttDashNetEventS2C> _cachedUnprocessedPerformHeadbuttDashEvents;
        private readonly CapacityList<PerformBarrelDashNetEventS2C> _cachedUnprocessedPerformBarrelDashEvents;
        private readonly CapacityList<PlayerSetMoveDestinationPointNetEventS2C> _cachedUnprocessedPlayerSetMoveDestinationPointEvents;
        private readonly CapacityList<HeadbuttHitEnemyNetEventS2C> _cachedUnprocessedHeadbuttHitEnemyEvents;
        private readonly CapacityList<DeactivateHeadbuttTalentNetEventS2C> _cachedUnprocessedDeactivateHeadbuttTalentEvents;
        private readonly CapacityList<CreateMagneticPullFieldNetEventS2C> _cachedUnprocessedCreateMagenticPullFieldEvents;
        private readonly CapacityList<LayChickenEggNetEventS2C> _cachedUnprocessedLayChickenEggEvents;
        private readonly CapacityList<ChickenEggHitNetEventS2C> _cachedUnprocessedChickenEggHitEvents;

        private readonly CapacityList<ActivateYearsOfPainTalentNetEventS2C> _cachedUnprocessedActivateYearsOfPainTalentEvents;
        private readonly CapacityList<PlayerLockOnTargetsChangedNetEventS2C> _cachedUnprocessedPlayerLockOnTargetsChangedNetEvents;
        private readonly CapacityList<PlayerLockedOnTargetHitNetEventS2C> _cachedUnprocessedPlayerLockedOnTargetHitNetEvents;
        private readonly CapacityList<PlayerPowerUpChangedNetEventS2C> _cachedUnprocessedPlayerPowerUpChangedNetEvents;
        private readonly CapacityList<ActivateSonicSlapNetEventS2C> _cachedUnprocessedActivateSonicSlapNetEvents;
        private readonly CapacityList<PerformGalacticPullNetEventS2C> _cachedUnprocessedPerformGalacticPullNetEvents;
        private readonly CapacityList<DeactivateGalacticForceFieldNetEventS2C> _cachedUnprocessedDeactivateGalacticForceFieldNetEvents;
        private readonly CapacityList<ActivateNukePowerUpNetEventS2C> _cachedUnprocessedActivateNukePowerUpNetEvents;
        private readonly CapacityList<DeactivateShufflePowerUpNetEventS2C> _cachedUnprocessedDeactivateShufflePowerUpNetEvents;
        private readonly CapacityList<ShuffleSwapPlayerPositionNetEventS2C> _cachedUnprocessedShuffleSwapPlayerPositionNetEvents;
        private readonly CapacityList<ActivateShuffleNetEventS2C> _cachedUnprocessedActivateShuffleNetEvents;
        private readonly CapacityList<StartPowerUpGrantingPhaseNetEventS2C> _cachedUnprocessedStartPowerUpGrantingPhaseNetEvents;
        private readonly CapacityList<EndPowerUpGrantingPhaseNetEventS2C> _cachedUnprocessedEndPowerUpGrantingPhaseNetEvents;
        private readonly ConcurrentPool<MatchFullTickPacketS2C> _fullTickPacketsPool;

        private int _largestPacketSizeInLast5Seconds;
        private int _averagePacketSizeReceived;
        private long _totalBytesReceived;
        private int _totalPacketsReceived;
        private float _lastLargestPacketResetTime;
        private GUIStyle _highVisStyle;
        private int _lastPacketSize;

        public PacketTypeS2C PacketType => PacketTypeS2C.MatchFullTick;
        public int LastProcessedTickFromServer { get; private set; }

        public MatchFullTickPacketsHandler(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, IClientNetworkManager networkManager,
            IMatchDataService matchDataService, ICachedPresentationEventsService cachedPresentationEventsService, ICommandFactory commandFactory, IUpdateSubscriptionService updateSubscriptionService, ILastFullSyncTickDataService lastFullSyncTickDataService,
            IInterpolationDecayService interpolationDecayService, INetworkDiagnosticsService networkDiagnosticsService)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _updateSubscriptionService = updateSubscriptionService;
            _lastFullSyncTickDataService = lastFullSyncTickDataService;
            _interpolationDecayService = interpolationDecayService;
            _networkDiagnosticsService = networkDiagnosticsService;
            _presentationNetEventsHandler = new PresentationMatchNetEventsHandler(matchDataService, cachedPresentationEventsService, commandFactory);
            _fullTickPacketsUnprocessedByLogic = new CapacityDict<int, MatchFullTickPacketS2C>(networkConfig.MaxCap.FullTickPacketsNetEvents);
            _fullTickPacketsUnprocessedByView = new CapacityDict<int, MatchFullTickPacketS2C>(networkConfig.MaxCap.FullTickPacketsNetEvents);
            _cachedUnprocessedPlayerRejoinedEvents = new CapacityList<PlayerRejoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents);
            _cachedUnprocessedBulletSpawnedEvents = new CapacityList<BulletSpawnNetEventS2C>(networkConfig.MaxCap.BulletSpawnNetEvents);
            _cachedUnprocessedPlayerTakeDamageEvents = new CapacityList<PlayerTakeDamageNetEventS2C>(networkConfig.MaxCap.PlayerTakeDamageNetEvents);
            _cachedUnprocessedPlayerDiedEvents = new CapacityList<PlayerDiedNetEventS2C>(networkConfig.MaxCap.PlayerDiedNetEvents);
            _cachedUnprocessedBulletDestroyedEvents = new CapacityList<BulletDestroyedNetEventS2C>(networkConfig.MaxCap.BulletDestroyedNetEvents);
            _cachedUnprocessedPlayerSwapEvents = new CapacityList<PlayersSwapNetEventS2C>(networkConfig.MaxCap.PlayerSwapNetEvents);
            _cachedUnprocessedTalentCardObtainedEvents = new CapacityList<TalentCardObtainedNetEventS2C>(networkConfig.MaxCap.TalentCardObtainedNetEvent);
            _cachedUnprocessedTalentCardHitEvents = new CapacityList<TalentCardHitNetEventS2C>(networkConfig.MaxCap.TalentCardHitNetEvents);
            _cachedUnprocessedPlayerSpinnedStartedEvents = new CapacityList<PlayerSpinnedStartedNetEventS2C>(networkConfig.MaxCap.PlayerSpinnedStartedNetEvents);
            _cachedUnprocessedPlayerSpinnedEndedEvents = new CapacityList<PlayerSpinnedEndedNetEventS2C>(networkConfig.MaxCap.PlayerSpinnedEndedNetEvents);
            _cachedUnprocessedPlayerStartedExposedToLavaEvents = new CapacityList<PlayerStartedExposedToLavaNetEventS2C>(networkConfig.MaxCap.PlayerStartedExposedToLavaNetEvents);
            _cachedUnprocessedPlayerEndedExposedToLavaEvents = new CapacityList<PlayerEndedExposedToLavaNetEventS2C>(networkConfig.MaxCap.PlayerEndedExposedToLavaNetEvents);
            _cachedUnprocessedPowerUpBallSpawnedEvents = new CapacityList<PowerUpBallSpawnedNetEventS2C>(networkConfig.MaxCap.PowerUpSpawnedNetEvents);
            _cachedUnprocessedPowerUpBallObtainedEvents = new CapacityList<PowerUpBallObtainedNetEventS2C>(networkConfig.MaxCap.PowerUpObtainedNetEvents);
            _cachedUnprocessedMoleSpawnedEvents = new CapacityList<MoleSpawnedNetEventS2C>(networkConfig.MaxCap.MoleSpawnedNetEvents);
            _cachedUnprocessedMoleKilledEvents = new CapacityList<MoleKilledNetEventS2C>(networkConfig.MaxCap.MoleKilledNetEvents);
            _cachedUnprocessedMoleExpiredEvents = new CapacityList<MoleExpiredNetEventS2C>(networkConfig.MaxCap.MoleExpiredNetEvents);
            _cachedUnprocessedGoldenMoleDamagedEvents = new CapacityList<GoldenMoleDamagedNetEventS2C>(networkConfig.MaxCap.GoldenMoleDamagedNetEvents);
            _cachedUnprocessedPlayerPassedScoreGateEvents = new CapacityList<PlayerPassedScoreGateNetEventS2C>(networkConfig.MaxCap.PlayerPassedScoreGateNetEvents);
            _cachedUnprocessedGateTrapClosingEvents = new CapacityList<GateTrapClosingNetEventS2C>(networkConfig.MaxCap.GateTrapClosingNetEvents);
            _cachedUnprocessedStageEndEvents = new CapacityList<StageEndNetEventS2C>(networkConfig.MaxCap.StageEndNetEvents);
            _cachedUnprocessedTeamLostEvents = new CapacityList<TeamLostNetEventS2C>(sharedGamePlayConfig.MaxTeamsAmount);
            _cachedUnprocessedTalentSwitchEvents = new CapacityList<TalentSwitchNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents);
            _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents = new CapacityList<EnvironmentSpringPlayerCollisionNetEventS2C>(networkConfig.MaxCap.EnvironmentSpringPlayerCollisionNetEvents);
            _cachedUnprocessedEnvironmentSpikePlayerCollisionEvents = new CapacityList<EnvironmentSpikePlayerCollisionNetEventS2C>(networkConfig.MaxCap.EnvironmentSpikePlayerCollisionNetEvents);
            _cachedUnprocessedGainBoltsEvents = new CapacityList<GainBoltsNetEventS2C>(networkConfig.MaxCap.GainBoltsNetEvents);
            _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents = new CapacityList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>(networkConfig.MaxCap.PlayerToEnvironmentTeleportGateCollisionNetEvents);
            _cachedUnprocessedPreparationPhaseEndedEvents = new CapacityList<PreparationPhaseEndedNetEventS2C>(networkConfig.MaxCap.PreparationPhaseEndedNetEvents);
            _cachedUnprocessedCreateSwapFieldEvents = new CapacityList<CreateSwapFieldNetEventS2C>(networkConfig.MaxCap.CreateSwapFieldNetEvents);
            _cachedUnprocessedDeactivateSwapTalentEvents = new CapacityList<DeactivateSwapTalentNetEventS2C>(networkConfig.MaxCap.DestroySwapFieldNetEvents);
            _cachedUnprocessedKOProjectHitPlayerEvents = new CapacityList<KOProjectHitPlayerNetEventS2C>(networkConfig.MaxCap.KOProjectHitPlayerNetEvents);
            _cachedUnprocessedCreateKOProjectileEvents = new CapacityList<CreateKOProjectileNetEventS2C>(networkConfig.MaxCap.CreateKOProjectileNetEvents);
            _cachedUnprocessedDeactivateKOTalentEvents = new CapacityList<DeactivateKOTalentNetEventS2C>(networkConfig.MaxCap.DeactivateKOTalentNetEvents);
            _cachedUnprocessedCreateGrapplingHookProjectileEvents = new CapacityList<CreateGrapplingHookProjectileNetEventS2C>(networkConfig.MaxCap.PlayerGrapplingHookShotNetEvents);
            _cachedUnprocessedGrapplingHookHitWallEvents = new CapacityList<GrapplingHookHitWallNetEventS2C>(networkConfig.MaxCap.PlayerGrapplingHookHitNetEvents);
            _cachedUnprocessedDeactivateGrapplingHookTalentEvents = new CapacityList<DeactivateGrapplingHookTalentNetEventS2C>(networkConfig.MaxCap.PlayerGrapplingHookDeactivatedNetEvents);
            _cachedUnprocessedCreateFishingRodProjectileEvents = new CapacityList<CreateFishingRodProjectileNetEventS2C>(networkConfig.MaxCap.CreateFishingRodProjectileNetEvents);
            _cachedUnprocessedFishingRodCaughtEnemyEvents = new CapacityList<FishingRodCaughtEnemyNetEventS2C>(networkConfig.MaxCap.FishingRodCaughtEnemyNetEvents);
            _cachedUnprocessedFishingRodTipHitWallEvents = new CapacityList<FishingRodTipHitWallNetEventS2C>(networkConfig.MaxCap.FishingRodTipHitWallNetEvents);
            _cachedUnprocessedFishingRodThrowEvents = new CapacityList<FishingRodThrowNetEventS2C>(networkConfig.MaxCap.FishingRodThrowNetEvents);
            _cachedUnprocessedDeactivateFishingRodTalentEvents = new CapacityList<DeactivateFishingRodTalentNetEventS2C>(networkConfig.MaxCap.DeactivateFishingRodTalentNetEvents);
            _cachedUnprocessedCreateSoulGhostEvents = new CapacityList<CreateSoulGhostNetEventS2C>(networkConfig.MaxCap.CreateSoulGhostNetEvents);
            _cachedUnprocessedDeactivateSoulTalentEvents = new CapacityList<DeactivateSoulTalentNetEventS2C>(networkConfig.MaxCap.DeactivateSoulTalentNetEvents);
            _cachedUnprocessedActivateRockTalentEvents = new CapacityList<ActivateRockTalentNetEventS2C>(networkConfig.MaxCap.ActivateRockTalentNetEvents);
            _cachedUnprocessedDeactivateRockTalentEvents = new CapacityList<DeactivateRockTalentNetEventS2C>(networkConfig.MaxCap.DeactivateRockTalentNetEvents);
            _cachedUnprocessedActivateFrozenTalentEvents = new CapacityList<ActivateFrozenTalentNetEventS2C>(networkConfig.MaxCap.ActivateFrozenTalentNetEvents);
            _cachedUnprocessedDeactivateFrozenTalentEvents = new CapacityList<DeactivateFrozenTalentNetEventS2C>(networkConfig.MaxCap.DeactivateFrozenTalentNetEvents);
            _cachedUnprocessedShootFrigidBlockEvents = new CapacityList<ShootFrigidBlockNetEventS2C>(networkConfig.MaxCap.ShootFrigidBlockNetEvents);
            _cachedUnprocessedDestroyFrigidBlockEvents = new CapacityList<DestroyFrigidBlockNetEventS2C>(networkConfig.MaxCap.DestroyFrigidBlockNetEvents);
            _cachedUnprocessedActivateSentryGunTalentEvents = new CapacityList<ActivateSentryGunTalentNetEventS2C>(networkConfig.MaxCap.ActivateSentryGunTalentNetEvents);
            _cachedUnprocessedDeactivateSentryGunTalentEvents = new CapacityList<DeactivateSentryGunTalentNetEventS2C>(networkConfig.MaxCap.DeactivateSentryGunTalentNetEvents);
            _cachedUnprocessedPerformDashPulseEvents = new CapacityList<PerformDashPulseNetEventS2C>(networkConfig.MaxCap.PerformDashPulseNetEvents);
            _cachedUnprocessedUpdatePlayerTalentStocksEvents = new CapacityList<UpdatePlayerTalentStocksNetEventS2C>(networkConfig.MaxCap.UpdatePlayerTalentStocksNetEvents);
            _cachedUnprocessedPlayerMaxShootCooldownChangedEvents = new CapacityList<PlayerMaxShootCooldownChangedNetEventS2C>(networkConfig.MaxCap.PlayerMaxShootCooldownChangedNetEvents);
            _cachedPlayerSelectedTalentFinishedCooldownLocalEvents = new CapacityList<PlayerSelectedTalentFinishedCooldownLocalEvent>(networkConfig.MaxCap.ConcurrentPlayers);
            _cachedUnprocessedActivateUmbrellaTalentEvents = new CapacityList<ActivateUmbrellaTalentNetEventS2C>(networkConfig.MaxCap.ActivateUmbrellaTalentNetEvents);
            _cachedUnprocessedDeactivateUmbrellaTalentEvents = new CapacityList<DeactivateUmbrellaTalentNetEventS2C>(networkConfig.MaxCap.DeactivateUmbrellaTalentNetEvents);
            _cachedUnprocessedActivateWaterGunTalentEvents = new CapacityList<ActivateWaterGunTalentNetEventS2C>(networkConfig.MaxCap.ActivateWaterGunTalentNetEvents);
            _cachedUnprocessedDeactivateWaterGunTalentEvents = new CapacityList<DeactivateWaterGunTalentNetEventS2C>(networkConfig.MaxCap.DeactivateWaterGunTalentNetEvents);
            _cachedUnprocessedActivateHeadbuttChargingEvents = new CapacityList<ActivateHeadbuttChargingNetEventS2C>(networkConfig.MaxCap.ActivateHeadbuttChargingNetEvents);
            _cachedUnprocessedPerformHeadbuttDashEvents = new CapacityList<PerformHeadbuttDashNetEventS2C>(networkConfig.MaxCap.PerformHeadbuttDashNetEvents);
            _cachedUnprocessedPerformBarrelDashEvents = new CapacityList<PerformBarrelDashNetEventS2C>(networkConfig.MaxCap.PerformBarrelDashNetEvents);
            _cachedUnprocessedPlayerSetMoveDestinationPointEvents = new CapacityList<PlayerSetMoveDestinationPointNetEventS2C>(networkConfig.MaxCap.PlayerSetMoveDestinationPointNetEvents);
            _cachedUnprocessedHeadbuttHitEnemyEvents = new CapacityList<HeadbuttHitEnemyNetEventS2C>(networkConfig.MaxCap.HeadbuttHitEnemyNetEvents);
            _cachedUnprocessedDeactivateHeadbuttTalentEvents = new CapacityList<DeactivateHeadbuttTalentNetEventS2C>(networkConfig.MaxCap.DeactivateHeadbuttTalentNetEvents);
            _cachedUnprocessedCreateMagenticPullFieldEvents = new CapacityList<CreateMagneticPullFieldNetEventS2C>(networkConfig.MaxCap.CreateMagneticPullFieldNetEvents);
            _cachedUnprocessedLayChickenEggEvents = new CapacityList<LayChickenEggNetEventS2C>(networkConfig.MaxCap.LayChickenEggNetEvents);
            _cachedUnprocessedChickenEggHitEvents = new CapacityList<ChickenEggHitNetEventS2C>(networkConfig.MaxCap.ChickenEggHitNetEvents);

            _cachedUnprocessedActivateYearsOfPainTalentEvents = new CapacityList<ActivateYearsOfPainTalentNetEventS2C>(networkConfig.MaxCap.ActivateYearsOfPainTalentNetEvents);
            _cachedUnprocessedPlayerLockOnTargetsChangedNetEvents = new CapacityList<PlayerLockOnTargetsChangedNetEventS2C>(networkConfig.MaxCap.PlayerLockOnTargetsChangedNetEvents);
            _cachedUnprocessedPlayerLockedOnTargetHitNetEvents = new CapacityList<PlayerLockedOnTargetHitNetEventS2C>(networkConfig.MaxCap.PlayerLockOnTargetHitNetEvents);
            _cachedUnprocessedPlayerPowerUpChangedNetEvents = new CapacityList<PlayerPowerUpChangedNetEventS2C>(networkConfig.MaxCap.PlayerPowerUpChangedNetEvents);
            _cachedUnprocessedActivateSonicSlapNetEvents = new CapacityList<ActivateSonicSlapNetEventS2C>(networkConfig.MaxCap.ActivateSonicSlapNetEvents);
            _cachedUnprocessedPerformGalacticPullNetEvents = new CapacityList<PerformGalacticPullNetEventS2C>(networkConfig.MaxCap.PerformGalacticPullNetEvents);
            _cachedUnprocessedDeactivateGalacticForceFieldNetEvents = new CapacityList<DeactivateGalacticForceFieldNetEventS2C>(networkConfig.MaxCap.DeactivateGalacticForceFieldNetEvents);
            _cachedUnprocessedActivateNukePowerUpNetEvents = new CapacityList<ActivateNukePowerUpNetEventS2C>(networkConfig.MaxCap.ActivateNukePowerUpNetEvents);
            _cachedUnprocessedDeactivateShufflePowerUpNetEvents = new CapacityList<DeactivateShufflePowerUpNetEventS2C>(networkConfig.MaxCap.ActivateShufflePowerUpNetEvents);
            _cachedUnprocessedShuffleSwapPlayerPositionNetEvents = new CapacityList<ShuffleSwapPlayerPositionNetEventS2C>(networkConfig.MaxCap.ShuffleSwapPlayerPositionNetEvents);
            _cachedUnprocessedActivateShuffleNetEvents = new CapacityList<ActivateShuffleNetEventS2C>(networkConfig.MaxCap.ActivateShufflePowerUpNetEvents);
            _cachedUnprocessedStartPowerUpGrantingPhaseNetEvents = new CapacityList<StartPowerUpGrantingPhaseNetEventS2C>(networkConfig.MaxCap.StartPowerUpGrantingPhaseNetEvents);
            _cachedUnprocessedEndPowerUpGrantingPhaseNetEvents = new CapacityList<EndPowerUpGrantingPhaseNetEventS2C>(networkConfig.MaxCap.EndPowerUpGrantingPhaseNetEvents);
            _fullTickPacketsPool = new ConcurrentPool<MatchFullTickPacketS2C>(() => new MatchFullTickPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig), networkConfig.MaxCap.FullTickPacketsNetEvents);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
            _updateSubscriptionService.RegisterGuiUpdatable(this);
        }

        public void ProcessStateLatestTick()
        {
            if (_fullTickPacketsUnprocessedByLogic.IsNullOrEmpty())
            {
                return;
            }

            var latestTickReceivedFromServer = GetLatestTickReceivedFromServer();
            var latestFullTickPacket = _fullTickPacketsUnprocessedByLogic[latestTickReceivedFromServer];
            var ignoreEventsNotAboveTick = Mathf.Max(LastProcessedTickFromServer, _lastFullSyncTickDataService.LastFullSyncTick);

            if (latestTickReceivedFromServer <= ignoreEventsNotAboveTick)
            {
                LogService.LogTopic("Didn't receive any state since last tick", LogTopicType.ClientNetwork);
                return;
            }
            
            ProcessLogicOfPacket(latestFullTickPacket, latestTickReceivedFromServer, ignoreEventsNotAboveTick);

            var ticksAdvancedSinceLastProcessedState = latestTickReceivedFromServer - LastProcessedTickFromServer;
            _interpolationDecayService.UpdateDecayBasedOnTicks(ticksAdvancedSinceLastProcessedState);
            _networkDiagnosticsService.OnStateProcessed(ticksAdvancedSinceLastProcessedState);
            LastProcessedTickFromServer = latestTickReceivedFromServer;

            foreach (var kvp in _fullTickPacketsUnprocessedByLogic)
            {
                var packetTick = kvp.Key;
                var doesPacketNeedToBeProcessByView = packetTick == latestTickReceivedFromServer;
                if (doesPacketNeedToBeProcessByView)
                {
                    _fullTickPacketsUnprocessedByView.Add(packetTick, latestFullTickPacket);
                }
                else
                {
                    _fullTickPacketsPool.Return(kvp.Value);
                }
            }

            _fullTickPacketsUnprocessedByLogic.Clear();
        }

        private int GetLatestTickReceivedFromServer()
        {
            var latestTick = int.MinValue;

            foreach (var kvp in _fullTickPacketsUnprocessedByLogic)
            {
                if (kvp.Key > latestTick)
                {
                    latestTick = kvp.Key;
                }
            }

            return latestTick;
        }

        private void ProcessLogicOfPacket(MatchFullTickPacketS2C latestFullTickPacket, int latestTickReceivedFromServer, int ignoreEventsNotAboveTick)
        {
            ProcessPlayerRejoinedEvents(latestFullTickPacket.PlayerJoinAcceptNetEvents, latestTickReceivedFromServer, ignoreEventsNotAboveTick);
            ProcessPlayersTalentsNormalCooldownsTimersIfEnded(latestTickReceivedFromServer);
            ProcessBulletSpawnedEvents(latestFullTickPacket.BulletSpawnNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerTakeDamageEvents(latestFullTickPacket.PlayerTakeDamageNetEvents, ignoreEventsNotAboveTick);
            ProcessBulletDestroyedEvents(latestFullTickPacket.BulletDestroyedNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerSwapEvents(latestFullTickPacket.PlayerSwapNetEvents, ignoreEventsNotAboveTick);
            ProcessTalentCardHitEvents(latestFullTickPacket.TalentCardHitNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerSpinnedStartedEvents(latestFullTickPacket.PlayerSpinnedStartedNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerSpinnedEndedEvents(latestFullTickPacket.PlayerSpinnedEndedNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerStartedExposedToLavaEvents(latestFullTickPacket.PlayerStartedExposedToLavaNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerEndedExposedToLavaEvents(latestFullTickPacket.PlayerEndedExposedToLavaNetEvents, ignoreEventsNotAboveTick);
            ProcessTalentCardObtainedEvents(latestFullTickPacket.TalentCardObtainedNetEvents, ignoreEventsNotAboveTick);
            ProcessPowerUpBallSpawnedEvents(latestFullTickPacket.PowerUpSpawnedNetEvents, ignoreEventsNotAboveTick);
            ProcessPowerUpBallObtainedEvents(latestFullTickPacket.PowerUpObtainedNetEvents, ignoreEventsNotAboveTick);
            ProcessMoleSpawnedEvents(latestFullTickPacket.MoleSpawnedNetEvents, ignoreEventsNotAboveTick);
            ProcessMoleKilledEvents(latestFullTickPacket.MoleKilledNetEvents, ignoreEventsNotAboveTick);
            ProcessMoleExpiredEvents(latestFullTickPacket.MoleExpiredNetEvents, ignoreEventsNotAboveTick);
            ProcessGoldenMoleDamagedEvents(latestFullTickPacket.GoldenMoleDamagedNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerPassedScoreGateEvents(latestFullTickPacket.PlayerPassedScoreGateNetEvents, ignoreEventsNotAboveTick);
            ProcessGateTrapClosingEvents(latestFullTickPacket.GateTrapClosingNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerDiedEvents(latestFullTickPacket.PlayerDiedNetEvents, ignoreEventsNotAboveTick);
            ProcessStageEndEvents(latestFullTickPacket.StageEndNetEvents, ignoreEventsNotAboveTick);
            ProcessTeamLostEvents(latestFullTickPacket.TeamLostNetEvents, ignoreEventsNotAboveTick);
            ProcessTalentSwitchEvents(latestFullTickPacket.TalentSwitchNetEvents, ignoreEventsNotAboveTick);
            ProcessGainBoltsEvents(latestFullTickPacket.GainBoltsNetEvents, ignoreEventsNotAboveTick);
            ProcessEnvironmentSpringPlayerCollisionEvents(latestFullTickPacket.EnvironmentSpringPlayerCollisionNetEvents, ignoreEventsNotAboveTick);
            ProcessEnvironmentSpikePlayerCollisionEvents(latestFullTickPacket.EnvironmentSpikePlayerCollisionNetEvents, ignoreEventsNotAboveTick);
            ProcessEnvironmentTeleportPlayerCollisionEvents(latestFullTickPacket.PlayerToEnvironmentTeleportGateCollisionNetEvents, ignoreEventsNotAboveTick);
            ProcessPreparationPhaseEndedEvents(latestFullTickPacket.PreparationPhaseEndedNetEvents, ignoreEventsNotAboveTick);
            ProcessCreateSwapFieldEvents(latestFullTickPacket.CreateSwapFieldNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateSwapTalentEvents(latestFullTickPacket.DestroySwapFieldNetEvents, ignoreEventsNotAboveTick);
            ProcessKOProjectHitPlayerEvents(latestFullTickPacket.KOProjectHitPlayerNetEvents, ignoreEventsNotAboveTick);
            ProcessCreateKOProjectileEvents(latestFullTickPacket.CreateKOProjectileNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateKOTalentEvents(latestFullTickPacket.DeactivateKOTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessCreateGrapplingHookProjectileEvents(latestFullTickPacket.CreateGrapplingHookProjectileNetEvents, ignoreEventsNotAboveTick);
            ProcessGrapplingHookHitWallEvents(latestFullTickPacket.GrapplingHookHitWallNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateGrapplingHookTalentEvents(latestFullTickPacket.DeactivateGrapplingHookTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateSentryGunTalentEvents(latestFullTickPacket.ActivateSentryGunTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateSentryGunTalentEvents(latestFullTickPacket.DeactivateSentryGunTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessPerformDashPulseEvents(latestFullTickPacket.PerformDashPulseNetEvents, ignoreEventsNotAboveTick);
            ProcessUpdatePlayerTalentStockEvents(latestFullTickPacket.UpdatePlayerTalentStocksNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerMaxShootCooldownChangedEvents(latestFullTickPacket.PlayerMaxShootCooldownChangedNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateUmbrellaTalentEvents(latestFullTickPacket.ActivateUmbrellaTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateUmbrellaTalentEvents(latestFullTickPacket.DeactivateUmbrellaTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateWaterGunTalentEvents(latestFullTickPacket.ActivateWaterGunTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateWaterGunTalentEvents(latestFullTickPacket.DeactivateWaterGunTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateHeadbuttChargingEvents(latestFullTickPacket.ActivateHeadbuttChargingNetEvents, ignoreEventsNotAboveTick);
            ProcessPerformHeadbuttDashEvents(latestFullTickPacket.PerformHeadbuttDashNetEvents, ignoreEventsNotAboveTick);
            ProcessPerformBarrelDashEvents(latestFullTickPacket.PerformBarrelDashNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerSetMoveDestinationPointEvents(latestFullTickPacket.PlayerSetMoveDestinationPointNetEvents, ignoreEventsNotAboveTick);
            ProcessHeadbuttHitEnemyEvents(latestFullTickPacket.HeadbuttHitEnemyNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateHeadbuttTalentEvents(latestFullTickPacket.DeactivateHeadbuttTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessCreateMagenticPullFieldEvents(latestFullTickPacket.CreateMagneticPullFieldNetEvents, ignoreEventsNotAboveTick);
            ProcessLayChickenEggEvents(latestFullTickPacket.LayChickenEggNetEvents, ignoreEventsNotAboveTick);
            ProcessChickenEggHitEvents(latestFullTickPacket.ChickenEggHitNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateYearsOfPainTalentEvents(latestFullTickPacket.ActivateYearsOfPainTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerLockOnTargetsChangedNetEvents(latestFullTickPacket.PlayerLockOnTargetsChangedNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerLockedOnTargetHitNetEvents(latestFullTickPacket.PlayerLockedOnTargetHitNetEvents, ignoreEventsNotAboveTick);
            ProcessPlayerPowerUpChangedNetEvents(latestFullTickPacket.PlayerPowerUpChangedNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateSonicSlapNetEvents(latestFullTickPacket.ActivateSonicSlapNetEvents, ignoreEventsNotAboveTick);
            ProcessPerformGalacticPullNetEvents(latestFullTickPacket.PerformGalacticPullNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateGalacticForceFieldNetEvents(latestFullTickPacket.DeactivateGalacticForceFieldNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateNukePowerUpNetEvents(latestFullTickPacket.ActivateNukePowerUpNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateShufflePowerUpNetEvents(latestFullTickPacket.DeactivateShufflePowerUpNetEvents, ignoreEventsNotAboveTick);
            ProcessShuffleSwapPlayerPositionNetEvents(latestFullTickPacket.ShuffleSwapPlayerPositionNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateShuffleNetEvents(latestFullTickPacket.ActivateShuffleNetEvents, ignoreEventsNotAboveTick);
            ProcessStartPowerUpGrantingPhaseNetEvents(latestFullTickPacket.StartPowerUpGrantingPhaseNetEvents, ignoreEventsNotAboveTick);
            ProcessEndPowerUpGrantingPhaseNetEvents(latestFullTickPacket.EndPowerUpGrantingPhaseNetEvents, ignoreEventsNotAboveTick);
            ProcessShootFrigidBlockEvents(latestFullTickPacket.ShootFrigidBlockNetEvents, ignoreEventsNotAboveTick);
            ProcessDestroyFrigidBlockEvents(latestFullTickPacket.DestroyFrigidBlockNetEvents, ignoreEventsNotAboveTick);
            ProcessCreateFishingRodProjectileEvents(latestFullTickPacket.CreateFishingRodProjectileNetEvents, ignoreEventsNotAboveTick);
            ProcessFishingRodCaughtEnemyEvents(latestFullTickPacket.FishingRodCaughtEnemyNetEvents, ignoreEventsNotAboveTick);
            ProcessFishingRodTipHitWallEvents(latestFullTickPacket.FishingRodTipHitWallNetEvents, ignoreEventsNotAboveTick);
            ProcessFishingRodThrowEvents(latestFullTickPacket.FishingRodThrowNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateFishingRodTalentEvents(latestFullTickPacket.DeactivateFishingRodTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessCreateSoulGhostEvents(latestFullTickPacket.CreateSoulGhostNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateSoulTalentEvents(latestFullTickPacket.DeactivateSoulTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateRockTalentEvents(latestFullTickPacket.ActivateRockTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateRockTalentEvents(latestFullTickPacket.DeactivateRockTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessActivateFrozenTalentEvents(latestFullTickPacket.ActivateFrozenTalentNetEvents, ignoreEventsNotAboveTick);
            ProcessDeactivateFrozenTalentEvents(latestFullTickPacket.DeactivateFrozenTalentNetEvents, ignoreEventsNotAboveTick);
            var simulationState = latestFullTickPacket.CurrentSimulationState;
            UpdatePlayersDeltas(simulationState);
            UpdateBulletsTransform();
            UpdatePowerUpBallsTransform(simulationState);
            UpdateRotatingWheels(latestTickReceivedFromServer);
            UpdateGateTraps(latestTickReceivedFromServer);
            UpdateKOProjectilesTransform(simulationState);
            UpdateGrapplingHookProjectilesTransform(simulationState);
            UpdateFishingRodTipsTransform(simulationState);
            UpdateSoulGhostsTransform(simulationState);
            UpdateFrigidBlocksTransform(simulationState);
            ReconcileScoreGatesFromState(simulationState);
        }

        /// <summary>
        /// the server doesn't send this to the client because we prefer to save this redundent bandwidth,
        /// so the client need to clear the cooldowns on its own.
        /// </summary>
        /// <param name="latestTickReceivedFromServer"></param>
        private void ProcessPlayersTalentsNormalCooldownsTimersIfEnded(int latestTickReceivedFromServer)
        {
            _cachedPlayerSelectedTalentFinishedCooldownLocalEvents.Clear();

            foreach (var playerModel in _matchDataService.Players)
            {
                for (int i = 0; i < playerModel.Spaceship.TalentsState.Talents.Count; i++)
                {
                    ref var talentsState = ref playerModel.Spaceship.TalentsState.Talents.Get(i);
                    var isNormalCooldownType = talentsState.CooldownType == TalentCooldownType.Normal;
                    if (!isNormalCooldownType)
                    {
                        continue;
                    }

                    var didCooldownEnd = talentsState.NormalCooldown.CooldownEndTick <= latestTickReceivedFromServer;
                    if (didCooldownEnd)
                    {
                        talentsState.ClearCooldown();
                        
                        var selectedTalentIndex = playerModel.Spaceship.TalentsState.SelectedTalentIndex;
                        var isSelectedTalent = selectedTalentIndex == i;
                        if (isSelectedTalent)
                        {
                            _cachedPlayerSelectedTalentFinishedCooldownLocalEvents.Add(new PlayerSelectedTalentFinishedCooldownLocalEvent(playerModel.PlayerId));
                        }
                    }
                }
            }
            
            if (!_cachedPlayerSelectedTalentFinishedCooldownLocalEvents.IsNullOrEmpty())
            {
                _presentationNetEventsHandler.ProcessPlayerSelectedTalentFinishedCooldownEvents(_cachedPlayerSelectedTalentFinishedCooldownLocalEvents);
            }
        }

        private void ProcessEnvironmentSpikePlayerCollisionEvents(FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C> environmentSpikePlayerCollisionNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedEnvironmentSpikePlayerCollisionEvents.Clear();

            foreach (var netEvent in environmentSpikePlayerCollisionNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedEnvironmentSpikePlayerCollisionEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedEnvironmentSpikePlayerCollisionEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedEnvironmentSpikePlayerCollisionEvents.Sort();
                _presentationNetEventsHandler.ProcessEnvironmentSpikePlayerCollisionEvents(_cachedUnprocessedEnvironmentSpikePlayerCollisionEvents);
            }
        }
        
        private void ProcessEnvironmentTeleportPlayerCollisionEvents(FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> playerToEnvironmentTeleportGateCollisionNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.Clear();

            foreach (var netEvent in playerToEnvironmentTeleportGateCollisionNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerToEnvironmentTeleportCollisionEvents(_cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents);
            }
        }

        private void ProcessPreparationPhaseEndedEvents(FixedUnorderedList<PreparationPhaseEndedNetEventS2C> preparationPhaseEndedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPreparationPhaseEndedEvents.Clear();

            foreach (var netEvent in preparationPhaseEndedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPreparationPhaseEndedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPreparationPhaseEndedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPreparationPhaseEndedEvents.Sort();
                _presentationNetEventsHandler.ProcessPreparationPhaseEndedEvents(_cachedUnprocessedPreparationPhaseEndedEvents);
            }
        }

        private void ProcessCreateSwapFieldEvents(FixedUnorderedList<CreateSwapFieldNetEventS2C> createSwapFieldNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedCreateSwapFieldEvents.Clear();

            foreach (var netEvent in createSwapFieldNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedCreateSwapFieldEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedCreateSwapFieldEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedCreateSwapFieldEvents.Sort();
                _presentationNetEventsHandler.ProcessCreateSwapFieldEvents(_cachedUnprocessedCreateSwapFieldEvents);
            }
        }

        private void ProcessDeactivateSwapTalentEvents(FixedUnorderedList<DeactivateSwapTalentNetEventS2C> deactivateSwapTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateSwapTalentEvents.Clear();

            foreach (var netEvent in deactivateSwapTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateSwapTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedDeactivateSwapTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateSwapTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateSwapTalentEvents(_cachedUnprocessedDeactivateSwapTalentEvents);
            }
        }

        private void ProcessKOProjectHitPlayerEvents(FixedUnorderedList<KOProjectHitPlayerNetEventS2C> koProjectHitPlayerNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedKOProjectHitPlayerEvents.Clear();

            foreach (var netEvent in koProjectHitPlayerNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedKOProjectHitPlayerEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedKOProjectHitPlayerEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedKOProjectHitPlayerEvents.Sort();
                _presentationNetEventsHandler.ProcessKOProjectHitPlayerEvents(_cachedUnprocessedKOProjectHitPlayerEvents);
            }
        }

        private void ProcessCreateKOProjectileEvents(FixedUnorderedList<CreateKOProjectileNetEventS2C> createKOProjectileNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedCreateKOProjectileEvents.Clear();

            foreach (var netEvent in createKOProjectileNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedCreateKOProjectileEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedCreateKOProjectileEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedCreateKOProjectileEvents.Sort();
                _presentationNetEventsHandler.ProcessCreateKOProjectileEvents(_cachedUnprocessedCreateKOProjectileEvents);
            }
        }

        private void ProcessCreateGrapplingHookProjectileEvents(FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedCreateGrapplingHookProjectileEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedCreateGrapplingHookProjectileEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedCreateGrapplingHookProjectileEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedCreateGrapplingHookProjectileEvents.Sort();
                _presentationNetEventsHandler.ProcessCreatePlayerGrapplingHookProjectileEvents(_cachedUnprocessedCreateGrapplingHookProjectileEvents);
            }
        }

        private void ProcessShootFrigidBlockEvents(FixedUnorderedList<ShootFrigidBlockNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedShootFrigidBlockEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedShootFrigidBlockEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedShootFrigidBlockEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedShootFrigidBlockEvents.Sort();
                _presentationNetEventsHandler.ProcessShootFrigidBlockEvents(_cachedUnprocessedShootFrigidBlockEvents);
            }
        }

        private void ProcessDestroyFrigidBlockEvents(FixedUnorderedList<DestroyFrigidBlockNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDestroyFrigidBlockEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDestroyFrigidBlockEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDestroyFrigidBlockEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDestroyFrigidBlockEvents.Sort();
                _presentationNetEventsHandler.ProcessDestroyFrigidBlockEvents(_cachedUnprocessedDestroyFrigidBlockEvents);
            }
        }

        private void ProcessGrapplingHookHitWallEvents(FixedUnorderedList<GrapplingHookHitWallNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedGrapplingHookHitWallEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedGrapplingHookHitWallEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedGrapplingHookHitWallEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedGrapplingHookHitWallEvents.Sort();
                _presentationNetEventsHandler.ProcessGrapplingHookHitWallEvents(_cachedUnprocessedGrapplingHookHitWallEvents);
            }
        }

        private void ProcessDeactivateGrapplingHookTalentEvents(FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateGrapplingHookTalentEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateGrapplingHookTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDeactivateGrapplingHookTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateGrapplingHookTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateGrapplingHookTalentEvents(_cachedUnprocessedDeactivateGrapplingHookTalentEvents);
            }
        }

        private void ProcessCreateFishingRodProjectileEvents(FixedUnorderedList<CreateFishingRodProjectileNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedCreateFishingRodProjectileEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedCreateFishingRodProjectileEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedCreateFishingRodProjectileEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedCreateFishingRodProjectileEvents.Sort();
                _presentationNetEventsHandler.ProcessCreateFishingRodProjectileEvents(_cachedUnprocessedCreateFishingRodProjectileEvents);
            }
        }

        private void ProcessFishingRodCaughtEnemyEvents(FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedFishingRodCaughtEnemyEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedFishingRodCaughtEnemyEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedFishingRodCaughtEnemyEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedFishingRodCaughtEnemyEvents.Sort();
                _presentationNetEventsHandler.ProcessFishingRodCaughtEnemyEvents(_cachedUnprocessedFishingRodCaughtEnemyEvents);
            }
        }

        private void ProcessFishingRodTipHitWallEvents(FixedUnorderedList<FishingRodTipHitWallNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedFishingRodTipHitWallEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedFishingRodTipHitWallEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedFishingRodTipHitWallEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedFishingRodTipHitWallEvents.Sort();
                _presentationNetEventsHandler.ProcessFishingRodTipHitWallEvents(_cachedUnprocessedFishingRodTipHitWallEvents);
            }
        }

        private void ProcessFishingRodThrowEvents(FixedUnorderedList<FishingRodThrowNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedFishingRodThrowEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedFishingRodThrowEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedFishingRodThrowEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedFishingRodThrowEvents.Sort();
                _presentationNetEventsHandler.ProcessFishingRodThrowEvents(_cachedUnprocessedFishingRodThrowEvents);
            }
        }

        private void ProcessDeactivateFishingRodTalentEvents(FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateFishingRodTalentEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateFishingRodTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDeactivateFishingRodTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateFishingRodTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateFishingRodTalentEvents(_cachedUnprocessedDeactivateFishingRodTalentEvents);
            }
        }

        private void ProcessCreateSoulGhostEvents(FixedUnorderedList<CreateSoulGhostNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedCreateSoulGhostEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedCreateSoulGhostEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedCreateSoulGhostEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedCreateSoulGhostEvents.Sort();
                _presentationNetEventsHandler.ProcessCreateSoulGhostEvents(_cachedUnprocessedCreateSoulGhostEvents);
            }
        }

        private void ProcessDeactivateSoulTalentEvents(FixedUnorderedList<DeactivateSoulTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateSoulTalentEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateSoulTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDeactivateSoulTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateSoulTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateSoulTalentEvents(_cachedUnprocessedDeactivateSoulTalentEvents);
            }
        }

        private void ProcessActivateRockTalentEvents(FixedUnorderedList<ActivateRockTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateRockTalentEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateRockTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedActivateRockTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateRockTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateRockTalentEvents(_cachedUnprocessedActivateRockTalentEvents);
            }
        }

        private void ProcessDeactivateRockTalentEvents(FixedUnorderedList<DeactivateRockTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateRockTalentEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateRockTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDeactivateRockTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateRockTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateRockTalentEvents(_cachedUnprocessedDeactivateRockTalentEvents);
            }
        }

        private void ProcessActivateFrozenTalentEvents(FixedUnorderedList<ActivateFrozenTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateFrozenTalentEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateFrozenTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedActivateFrozenTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateFrozenTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateFrozenTalentEvents(_cachedUnprocessedActivateFrozenTalentEvents);
            }
        }

        private void ProcessDeactivateFrozenTalentEvents(FixedUnorderedList<DeactivateFrozenTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateFrozenTalentEvents.Clear();
            var span = events.AsSpan();
            foreach (var netEvent in span)
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateFrozenTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDeactivateFrozenTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateFrozenTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateFrozenTalentEvents(_cachedUnprocessedDeactivateFrozenTalentEvents);
            }
        }

        private void ProcessDeactivateKOTalentEvents(FixedUnorderedList<DeactivateKOTalentNetEventS2C> deactivateKOTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateKOTalentEvents.Clear();

            foreach (var netEvent in deactivateKOTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateKOTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedDeactivateKOTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateKOTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateKOTalentEvents(_cachedUnprocessedDeactivateKOTalentEvents);
            }
        }

        private void ProcessPerformDashPulseEvents(FixedUnorderedList<PerformDashPulseNetEventS2C> performDashPulseNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPerformDashPulseEvents.Clear();

            foreach (var netEvent in performDashPulseNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPerformDashPulseEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPerformDashPulseEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPerformDashPulseEvents.Sort();
                _presentationNetEventsHandler.ProcessPerformDashPulseEvents(_cachedUnprocessedPerformDashPulseEvents);
            }
        }

        private void ProcessActivateUmbrellaTalentEvents(FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C> activateUmbrellaTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateUmbrellaTalentEvents.Clear();

            foreach (var netEvent in activateUmbrellaTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateUmbrellaTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedActivateUmbrellaTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateUmbrellaTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateUmbrellaTalentEvents(_cachedUnprocessedActivateUmbrellaTalentEvents);
            }
        }

        private void ProcessDeactivateUmbrellaTalentEvents(FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C> deactivateUmbrellaTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateUmbrellaTalentEvents.Clear();

            foreach (var netEvent in deactivateUmbrellaTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateUmbrellaTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedDeactivateUmbrellaTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateUmbrellaTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateUmbrellaTalentEvents(_cachedUnprocessedDeactivateUmbrellaTalentEvents);
            }
        }

        private void ProcessActivateWaterGunTalentEvents(FixedUnorderedList<ActivateWaterGunTalentNetEventS2C> activateWaterGunTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateWaterGunTalentEvents.Clear();

            foreach (var netEvent in activateWaterGunTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateWaterGunTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedActivateWaterGunTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateWaterGunTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateWaterGunTalentEvents(_cachedUnprocessedActivateWaterGunTalentEvents);
            }
        }

        private void ProcessDeactivateWaterGunTalentEvents(FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C> deactivateWaterGunTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateWaterGunTalentEvents.Clear();

            foreach (var netEvent in deactivateWaterGunTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateWaterGunTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedDeactivateWaterGunTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateWaterGunTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateWaterGunTalentEvents(_cachedUnprocessedDeactivateWaterGunTalentEvents);
            }
        }

        private void ProcessUpdatePlayerTalentStockEvents(FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C> updatePlayerTalentStocksNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedUpdatePlayerTalentStocksEvents.Clear();

            foreach (var netEvent in updatePlayerTalentStocksNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedUpdatePlayerTalentStocksEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedUpdatePlayerTalentStocksEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedUpdatePlayerTalentStocksEvents.Sort();
                _presentationNetEventsHandler.ProcessUpdatePlayerTalentStocksEvents(_cachedUnprocessedUpdatePlayerTalentStocksEvents);
            }
        }

        private void ProcessPlayerMaxShootCooldownChangedEvents(FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerMaxShootCooldownChangedEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerMaxShootCooldownChangedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerMaxShootCooldownChangedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerMaxShootCooldownChangedEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerMaxShootCooldownChangedEvents(_cachedUnprocessedPlayerMaxShootCooldownChangedEvents);
            }
        }

        private void ProcessCreateMagenticPullFieldEvents(FixedUnorderedList<CreateMagneticPullFieldNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedCreateMagenticPullFieldEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedCreateMagenticPullFieldEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedCreateMagenticPullFieldEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedCreateMagenticPullFieldEvents.Sort();
                _presentationNetEventsHandler.ProcessCreateMagenticPullFieldEvents(_cachedUnprocessedCreateMagenticPullFieldEvents);
            }
        }

        private void UpdateRotatingWheels(int tick)
        {
            if (_matchDataService.IsInPreparationPhase)
            {
                return;
            }
            
            var calculationTick = tick - _matchDataService.PreperationPhaseEndedOnTick;
            var deltaTime = _networkConfig.DeltaTime;

            foreach (var wheelModel in _matchDataService.RotatingWheels)
            {
                var wheelCenter = wheelModel.CenterPosition;
                var rotationSpeed = wheelModel.RotationSpeed;

                foreach (var wallId in wheelModel.WallIds)
                {
                    var wallModel = _matchDataService.GetEnvironmentWall(wallId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, wallModel.LocalPosition, 0,
                        out var worldPos, out var worldRot
                    );

                    wallModel.WorldPosition = worldPos;
                    wallModel.WorldRotationAngle = worldRot;
                }


                foreach (var lavaWallId in wheelModel.LavaWallIds)
                {
                    var lavaWallModel = _matchDataService.GetEnvironmentLavaWall(lavaWallId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, lavaWallModel.LocalPosition, 0,
                        out var worldPos, out var worldRot
                    );

                    lavaWallModel.WorldPosition = worldPos;
                    lavaWallModel.WorldRotationAngle = worldRot;
                }


                foreach (var springId in wheelModel.SpringIds)
                {
                    var springModel = _matchDataService.GetEnvironmentSpring(springId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, springModel.LocalPosition, springModel.LocalRotationAngle,
                        out var worldPos, out var worldRot
                    );

                    springModel.WorldPosition = worldPos;
                    springModel.WorldRotationAngle = worldRot;
                }

                foreach (var spikeId in wheelModel.SpikeIds)
                {
                    var spikeModel = _matchDataService.GetEnvironmentSpike(spikeId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, spikeModel.LocalPosition, spikeModel.LocalRotationAngle,
                        out var worldPos, out var worldRot
                    );

                    spikeModel.WorldPosition = worldPos;
                    spikeModel.WorldRotationAngle = worldRot;
                }

                foreach (var teleportGate in wheelModel.TeleportGates)
                {
                    var teleportPairModel = _matchDataService.GetTeleportPair(teleportGate.BelongToPairId);
                    var gateModel = teleportGate.IsGateA ? teleportPairModel.GateA : teleportPairModel.GateB;

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, gateModel.LocalPosition, gateModel.LocalRotation,
                        out var worldPosA, out var worldRotA
                    );

                    gateModel.WorldPosition = worldPosA;
                    gateModel.WorldRotation = worldRotA;
                }
            }
        }

        // The GateTrapClosing event is what starts a swing on the exact tick, and the rest of the cycle - staying closed,
        // opening and the open cooldown - is played out here from the same authored durations the server used. A client
        // that lost the event is put back in line by ReconcileGateTrapsFromState instead of staying wrong all stage.
        private void UpdateGateTraps(int tick)
        {
            if (_matchDataService.GateTraps.Count == 0)
            {
                return;
            }

            var wheelCalculationTick = _matchDataService.IsInPreparationPhase ? 0 : tick - _matchDataService.PreperationPhaseEndedOnTick;
            var deltaTime = _networkConfig.DeltaTime;

            foreach (var gateTrap in _matchDataService.GateTraps)
            {
                gateTrap.StepToTick(tick, wheelCalculationTick, deltaTime, _matchDataService.GetRotatingWheelOfGateTrap(gateTrap));
            }
        }

        private void UpdatePowerUpBallsTransform(MatchSimulationStateS2C simulationState)
        {
            foreach (var powerUpBallModel in _matchDataService.PowerUpBalls)
            {
                var powerUpBallById = simulationState.GetPowerUpBallById(powerUpBallModel.Id);
                powerUpBallModel.Position = powerUpBallById.Position.ToUnityVector2();
            }
        }

        private void ProcessTalentCardHitEvents(FixedUnorderedList<TalentCardHitNetEventS2C> talentCardHitNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedTalentCardHitEvents.Clear();

            foreach (var netEvent in talentCardHitNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedTalentCardHitEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTalentCardHitEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTalentCardHitEvents.Sort();
                _presentationNetEventsHandler.ProcessTalentCardHitEvents(_cachedUnprocessedTalentCardHitEvents);
            }
        }

        private void ProcessPlayerSpinnedStartedEvents(FixedUnorderedList<PlayerSpinnedStartedNetEventS2C> playerSpinnedStartedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerSpinnedStartedEvents.Clear();

            foreach (var netEvent in playerSpinnedStartedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerSpinnedStartedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerSpinnedStartedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerSpinnedStartedEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerSpinnedStartedEvents(_cachedUnprocessedPlayerSpinnedStartedEvents);
            }
        }

        private void ProcessPlayerSpinnedEndedEvents(FixedUnorderedList<PlayerSpinnedEndedNetEventS2C> playerSpinnedEndedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerSpinnedEndedEvents.Clear();

            foreach (var netEvent in playerSpinnedEndedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerSpinnedEndedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerSpinnedEndedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerSpinnedEndedEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerSpinnedEndedEvents(_cachedUnprocessedPlayerSpinnedEndedEvents);
            }
        }

        private void ProcessPlayerStartedExposedToLavaEvents(FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C> playerStartedExposedToLavaNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerStartedExposedToLavaEvents.Clear();

            foreach (var netEvent in playerStartedExposedToLavaNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerStartedExposedToLavaEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerStartedExposedToLavaEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerStartedExposedToLavaEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerStartedExposedToLavaEvents(_cachedUnprocessedPlayerStartedExposedToLavaEvents);
            }
        }

        private void ProcessPlayerEndedExposedToLavaEvents(FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C> playerEndedExposedToLavaNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerEndedExposedToLavaEvents.Clear();

            foreach (var netEvent in playerEndedExposedToLavaNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerEndedExposedToLavaEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerEndedExposedToLavaEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerEndedExposedToLavaEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerEndedExposedToLavaEvents(_cachedUnprocessedPlayerEndedExposedToLavaEvents);
            }
        }

        private void ProcessTalentCardObtainedEvents(FixedClassUnorderedList<TalentCardObtainedNetEventS2C> talentCardObtainedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedTalentCardObtainedEvents.Clear();

            foreach (var netEvent in talentCardObtainedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedTalentCardObtainedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTalentCardObtainedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTalentCardObtainedEvents.Sort();
                _presentationNetEventsHandler.ProcessTalentCardObtainedEvents(_cachedUnprocessedTalentCardObtainedEvents);
            }
        }

        private void ProcessPlayerSwapEvents(FixedUnorderedList<PlayersSwapNetEventS2C> playerSwapNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerSwapEvents.Clear();

            foreach (var netEvent in playerSwapNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerSwapEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerSwapEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerSwapEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerSwapEvents(_cachedUnprocessedPlayerSwapEvents);
            }
        }

        private void ProcessBulletDestroyedEvents(FixedUnorderedList<BulletDestroyedNetEventS2C> bulletDestroyedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedBulletDestroyedEvents.Clear();

            foreach (var netEvent in bulletDestroyedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedBulletDestroyedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedBulletDestroyedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedBulletDestroyedEvents.Sort();
                _presentationNetEventsHandler.ProcessBulletDestroyedEvents(_cachedUnprocessedBulletDestroyedEvents);
            }
        }

        private void ProcessPlayerTakeDamageEvents(FixedUnorderedList<PlayerTakeDamageNetEventS2C> playerTakeDamageNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerTakeDamageEvents.Clear();

            foreach (var netEvent in playerTakeDamageNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerTakeDamageEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerTakeDamageEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerTakeDamageEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerTakeDamageEvents(_cachedUnprocessedPlayerTakeDamageEvents);
            }
        }

        private void ProcessPlayerDiedEvents(FixedUnorderedList<PlayerDiedNetEventS2C> playerDiedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerDiedEvents.Clear();

            foreach (var netEvent in playerDiedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerDiedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerDiedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerDiedEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerDiedEvents(_cachedUnprocessedPlayerDiedEvents);
            }
        }


        private void ProcessPlayerRejoinedEvents(FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C> playerRejoinAcceptNetEvents, int lastProcessedTickFromServer, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerRejoinedEvents.Clear();

            foreach (var netEvent in playerRejoinAcceptNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerRejoinedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerRejoinedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerRejoinedEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerRejoinedEvents(_cachedUnprocessedPlayerRejoinedEvents, lastProcessedTickFromServer);
            }
        }

        private void ProcessBulletSpawnedEvents(FixedUnorderedList<BulletSpawnNetEventS2C> bulletSpawnNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedBulletSpawnedEvents.Clear();

            foreach (var netEvent in bulletSpawnNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedBulletSpawnedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedBulletSpawnedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedBulletSpawnedEvents.Sort();
                _presentationNetEventsHandler.ProcessBulletSpawnEvents(_cachedUnprocessedBulletSpawnedEvents);
            }
        }
        
        private void ProcessPowerUpBallSpawnedEvents(FixedUnorderedList<PowerUpBallSpawnedNetEventS2C> powerUpBallSpawnNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPowerUpBallSpawnedEvents.Clear();

            foreach (var netEvent in powerUpBallSpawnNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPowerUpBallSpawnedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPowerUpBallSpawnedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPowerUpBallSpawnedEvents.Sort();
                _presentationNetEventsHandler.ProcessPowerUpSpawnedEvents(_cachedUnprocessedPowerUpBallSpawnedEvents);
            }
        }
        
        private void ProcessPowerUpBallObtainedEvents(FixedUnorderedList<PowerUpBallObtainedNetEventS2C> powerUpBallObtainedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPowerUpBallObtainedEvents.Clear();

            foreach (var netEvent in powerUpBallObtainedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPowerUpBallObtainedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPowerUpBallObtainedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPowerUpBallObtainedEvents.Sort();
                _presentationNetEventsHandler.ProcessPowerUpObtainedEvents(_cachedUnprocessedPowerUpBallObtainedEvents);
            }
        }

        private void ProcessMoleSpawnedEvents(FixedUnorderedList<MoleSpawnedNetEventS2C> moleSpawnedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedMoleSpawnedEvents.Clear();

            foreach (var netEvent in moleSpawnedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedMoleSpawnedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedMoleSpawnedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedMoleSpawnedEvents.Sort();
                _presentationNetEventsHandler.ProcessMoleSpawnedEvents(_cachedUnprocessedMoleSpawnedEvents);
            }
        }

        private void ProcessMoleKilledEvents(FixedUnorderedList<MoleKilledNetEventS2C> moleKilledNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedMoleKilledEvents.Clear();

            foreach (var netEvent in moleKilledNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedMoleKilledEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedMoleKilledEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedMoleKilledEvents.Sort();
                _presentationNetEventsHandler.ProcessMoleKilledEvents(_cachedUnprocessedMoleKilledEvents);
            }
        }

        private void ProcessPlayerPassedScoreGateEvents(FixedUnorderedList<PlayerPassedScoreGateNetEventS2C> playerPassedScoreGateNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerPassedScoreGateEvents.Clear();

            foreach (var netEvent in playerPassedScoreGateNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerPassedScoreGateEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerPassedScoreGateEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerPassedScoreGateEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerPassedScoreGateEvents(_cachedUnprocessedPlayerPassedScoreGateEvents);
            }
        }

        private void ProcessGateTrapClosingEvents(FixedUnorderedList<GateTrapClosingNetEventS2C> gateTrapClosingNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedGateTrapClosingEvents.Clear();

            foreach (var netEvent in gateTrapClosingNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedGateTrapClosingEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedGateTrapClosingEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedGateTrapClosingEvents.Sort();
                _presentationNetEventsHandler.ProcessGateTrapClosingEvents(_cachedUnprocessedGateTrapClosingEvents);
            }
        }

        private void ProcessMoleExpiredEvents(FixedUnorderedList<MoleExpiredNetEventS2C> moleExpiredNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedMoleExpiredEvents.Clear();

            foreach (var netEvent in moleExpiredNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedMoleExpiredEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedMoleExpiredEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedMoleExpiredEvents.Sort();
                _presentationNetEventsHandler.ProcessMoleExpiredEvents(_cachedUnprocessedMoleExpiredEvents);
            }
        }

        private void ProcessGoldenMoleDamagedEvents(FixedUnorderedList<GoldenMoleDamagedNetEventS2C> goldenMoleDamagedNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedGoldenMoleDamagedEvents.Clear();

            foreach (var netEvent in goldenMoleDamagedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedGoldenMoleDamagedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedGoldenMoleDamagedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedGoldenMoleDamagedEvents.Sort();
                _presentationNetEventsHandler.ProcessGoldenMoleDamagedEvents(_cachedUnprocessedGoldenMoleDamagedEvents);
            }
        }

        private void ProcessTeamLostEvents(FixedUnorderedList<TeamLostNetEventS2C> teamLostNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedTeamLostEvents.Clear();

            foreach (var netEvent in teamLostNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedTeamLostEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTeamLostEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTeamLostEvents.Sort();
                _presentationNetEventsHandler.ProcessTeamLostEvents(_cachedUnprocessedTeamLostEvents);
            }
        }

        private void ProcessTalentSwitchEvents(FixedUnorderedList<TalentSwitchNetEventS2C> talentSwitchNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedTalentSwitchEvents.Clear();

            foreach (var netEvent in talentSwitchNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedTalentSwitchEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTalentSwitchEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTalentSwitchEvents.Sort();
                _presentationNetEventsHandler.ProcessTalentSwitchEvents(_cachedUnprocessedTalentSwitchEvents);
            }
        }

        private void ProcessEnvironmentSpringPlayerCollisionEvents(FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C> environmentSpringPlayerCollisionNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.Clear();

            foreach (var netEvent in environmentSpringPlayerCollisionNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.Sort();
                _presentationNetEventsHandler.ProcessEnvironmentSpringPlayerCollisionEvents(_cachedUnprocessedEnvironmentSpringPlayerCollisionEvents);
            }
        }

        private void ProcessStageEndEvents(FixedClassUnorderedList<StageEndNetEventS2C> stageEndNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedStageEndEvents.Clear();

            foreach (var netEvent in stageEndNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedStageEndEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedStageEndEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedStageEndEvents.Sort();
                _presentationNetEventsHandler.ProcessStageEndEvents(_cachedUnprocessedStageEndEvents);
            }
        }

        private void ProcessGainBoltsEvents(FixedUnorderedList<GainBoltsNetEventS2C> gainBoltsNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedGainBoltsEvents.Clear();

            foreach (var netEvent in gainBoltsNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedGainBoltsEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedGainBoltsEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedGainBoltsEvents.Sort();
                _presentationNetEventsHandler.ProcessGainBoltsNetEvents(_cachedUnprocessedGainBoltsEvents);
            }
        }
        
        private void ProcessPlayerLockOnTargetsChangedNetEvents(FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerLockOnTargetsChangedNetEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerLockOnTargetsChangedNetEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerLockOnTargetsChangedNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerLockOnTargetsChangedNetEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerLockOnTargetsChangedEvents(_cachedUnprocessedPlayerLockOnTargetsChangedNetEvents);
            }
        }

        private void ProcessPlayerLockedOnTargetHitNetEvents(FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerLockedOnTargetHitNetEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerLockedOnTargetHitNetEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerLockedOnTargetHitNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerLockedOnTargetHitNetEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerLockedOnTargetHitEvents(_cachedUnprocessedPlayerLockedOnTargetHitNetEvents);
            }
        }

        private void ProcessPlayerPowerUpChangedNetEvents(FixedUnorderedList<PlayerPowerUpChangedNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerPowerUpChangedNetEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerPowerUpChangedNetEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerPowerUpChangedNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerPowerUpChangedNetEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerPowerUpChangedEvents(_cachedUnprocessedPlayerPowerUpChangedNetEvents);
            }
        }

        private void ProcessActivateSonicSlapNetEvents(FixedClassUnorderedList<ActivateSonicSlapNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateSonicSlapNetEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateSonicSlapNetEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedActivateSonicSlapNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateSonicSlapNetEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateSonicSlapEvents(_cachedUnprocessedActivateSonicSlapNetEvents);
            }
        }

        private void ProcessStartPowerUpGrantingPhaseNetEvents(FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedStartPowerUpGrantingPhaseNetEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedStartPowerUpGrantingPhaseNetEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedStartPowerUpGrantingPhaseNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedStartPowerUpGrantingPhaseNetEvents.Sort();
                _presentationNetEventsHandler.ProcessStartPowerUpGrantingPhaseEvents(_cachedUnprocessedStartPowerUpGrantingPhaseNetEvents);
            }
        }

        private void ProcessEndPowerUpGrantingPhaseNetEvents(FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedEndPowerUpGrantingPhaseNetEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedEndPowerUpGrantingPhaseNetEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedEndPowerUpGrantingPhaseNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedEndPowerUpGrantingPhaseNetEvents.Sort();
                _presentationNetEventsHandler.ProcessEndPowerUpGrantingPhaseEvents(_cachedUnprocessedEndPowerUpGrantingPhaseNetEvents);
            }
        }

        private void ProcessActivateYearsOfPainTalentEvents(FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateYearsOfPainTalentEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateYearsOfPainTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedActivateYearsOfPainTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateYearsOfPainTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateYearsOfPainTalentEvents(_cachedUnprocessedActivateYearsOfPainTalentEvents);
            }
        }

        private void UpdatePlayersDeltas(MatchSimulationStateS2C simulationState)
        {
            foreach (var playerModel in _matchDataService.Players)
            {
                var playerState = simulationState.GetPlayerById(playerModel.PlayerId);
                playerModel.Spaceship.Transform.Position = playerState.Spaceship.Transform.Position;
                playerModel.Spaceship.Transform.Direction = playerState.Spaceship.Transform.Direction;
                playerModel.Spaceship.Shoot.CooldownSecondsLeft = playerState.Spaceship.Shoot.CooldownSecondsLeft;
                playerModel.Spaceship.AimDirection = playerState.Spaceship.AimDirection;
                playerModel.Spaceship.AssistArrowType = playerState.Spaceship.AssistArrowType;
            }
        }

        private void UpdateBulletsTransform()
        {
            foreach (var bullet in _matchDataService.Bullets)
            {
                bullet.Position = TickUtils.GetPositionInTick(bullet.SpawnTick, LastProcessedTickFromServer, bullet.PoisitionInSpawnTick, bullet.Velocity, _networkConfig.DeltaTime);
            }
        }

        private void UpdateGrapplingHookProjectilesTransform(MatchSimulationStateS2C simulationState)
        {
            foreach (var hook in _matchDataService.GrapplingHookProjectiles)
            {
                if (simulationState.TryGetGrapplingHookProjectileById(hook.Id, out var state))
                {
                    hook.Position = state.Position.ToUnityVector2();
                }
            }
        }

        private void UpdateFishingRodTipsTransform(MatchSimulationStateS2C simulationState)
        {
            foreach (var tip in _matchDataService.FishingRodTips)
            {
                if (simulationState.TryGetFishingRodProjectileById(tip.Id, out var state))
                {
                    tip.Position = state.Position.ToUnityVector2();
                    tip.EnemyCaughtArrowDirection = state.EnemyCaughtArrowDirection.ToUnityVector2();
                }
            }
        }

        private void UpdateSoulGhostsTransform(MatchSimulationStateS2C simulationState)
        {
            foreach (var ghost in _matchDataService.SoulGhosts)
            {
                if (simulationState.TryGetSoulGhostById(ghost.Id, out var state))
                {
                    ghost.Position = state.Position.ToUnityVector2();
                }
            }
        }

        private void UpdateKOProjectilesTransform(MatchSimulationStateS2C simulationState)
        {
            foreach (var koProjectile in _matchDataService.KOProjectiles)
            {
                var koProjectileState = simulationState.GetKOProjectileById(koProjectile.Id);
                koProjectile.Position = koProjectileState.Position.ToUnityVector2();
                koProjectile.Rotation = koProjectileState.Rotation.ToUnityVector2();
            }
        }

        private void UpdateFrigidBlocksTransform(MatchSimulationStateS2C simulationState)
        {
            foreach (var frigidBlock in _matchDataService.FrigidBlocks)
            {
                if (simulationState.TryGetFrigidBlockById(frigidBlock.Id, out var state))
                {
                    frigidBlock.Position = state.Position.ToUnityVector2();
                    frigidBlock.Rotation = state.Rotation.ToUnityVector2();
                }
            }
        }

        // Score gates come from the layout (no spawn event). Both the model and the view are built by the full-state
        // sync (SyncMatchSimulationStateCommand); here we only keep the transform of an already-created gate in sync as
        // it is shoved around. A gate not found yet just hasn't been synced this session, so we skip it rather than add a
        // viewless model - the next full sync creates both halves together.
        private void ReconcileScoreGatesFromState(MatchSimulationStateS2C simulationState)
        {
            foreach (var scoreGateState in simulationState.ScoreGates.AsSpan())
            {
                if (_matchDataService.TryGetScoreGate(scoreGateState.Id, out var scoreGateModel))
                {
                    scoreGateModel.Position = scoreGateState.Position.ToUnityVector2();
                    scoreGateModel.Rotation = scoreGateState.Rotation.ToUnityVector2();
                }
            }
        }
        
        public void OnPacketReceived(NetDataReader reader)
        {
            var packetSize = reader.RawDataSize;
            _totalPacketsReceived++;
            _totalBytesReceived += packetSize;
            _lastPacketSize = packetSize;
            _averagePacketSizeReceived = (int)(_totalBytesReceived / _totalPacketsReceived);
            
            if (Time.realtimeSinceStartup - _lastLargestPacketResetTime > 5f)
            {
                _largestPacketSizeInLast5Seconds = packetSize;
                _lastLargestPacketResetTime = Time.realtimeSinceStartup;
            }
            else if (packetSize > _largestPacketSizeInLast5Seconds)
            {
                _largestPacketSizeInLast5Seconds = packetSize;
            }

            _networkDiagnosticsService.OnFullTickPacketReceived();

            var newPacket = _fullTickPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnFullTickReceived(newPacket);
        }

        public void ClearUnprocessedPacketsByView()
        {
            foreach (var kvp in _fullTickPacketsUnprocessedByView)
            {
                _fullTickPacketsPool.Return(kvp.Value);   
            }
            
            _fullTickPacketsUnprocessedByView.Clear();
        }
        
        // UDP can deliver the same tick twice, and an already buffered tick has nothing new to tell us,
        // so the duplicate goes straight back to the pool instead of throwing out of PollEvents.
        private void OnFullTickReceived(MatchFullTickPacketS2C fullTickPacket)
        {
            LogService.LogTopic("FullTickPacket accepted received", LogTopicType.ClientNetwork);
            var tick = fullTickPacket.Tick;

            if (!_fullTickPacketsUnprocessedByLogic.TryAdd(tick, fullTickPacket))
            {
                _fullTickPacketsPool.Return(fullTickPacket);
            }
        }

        public void InitExitPoint()
        {
            UnregisterListeners();
        }

        private void UnregisterListeners()
        {
            _networkManager.UnregisterPacketsObserver(this);
            _updateSubscriptionService.UnregisterGuiUpdatable(this);
        }

        private void ProcessActivateSentryGunTalentEvents(FixedUnorderedList<ActivateSentryGunTalentNetEventS2C> activateSentryGunTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateSentryGunTalentEvents.Clear();
            foreach (var netEvent in activateSentryGunTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateSentryGunTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedActivateSentryGunTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateSentryGunTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateSentryGunTalentEvents(_cachedUnprocessedActivateSentryGunTalentEvents);
            }
        }

        private void ProcessDeactivateSentryGunTalentEvents(FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C> deactivateSentryGunTalentNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateSentryGunTalentEvents.Clear();
            foreach (var netEvent in deactivateSentryGunTalentNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateSentryGunTalentEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedDeactivateSentryGunTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateSentryGunTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateSentryGunTalentEvents(_cachedUnprocessedDeactivateSentryGunTalentEvents);
            }
        }

        public void ManagedOnGUI()
        {
            InitStyles();
            GUILayout.Box($"Ping: {_networkManager.Ping}, Last packet: {_lastPacketSize}b, Average: {_averagePacketSizeReceived}b, largest in last 5 seconds: {_largestPacketSizeInLast5Seconds}b", _highVisStyle);
            GUILayout.Box($"Interpolation decay: {_interpolationDecayService.CurrentDecay:0.0} (drops below the configured value while state packets arrive in bursts)", _highVisStyle);
            GUILayout.Box(_networkDiagnosticsService.LastReportText, _highVisStyle);
        }
        
        private void InitStyles()
        {
            if (_highVisStyle == null)
            {
                _highVisStyle = new GUIStyle(GUI.skin.box); 
                _highVisStyle.normal.background = Texture2D.whiteTexture;
                _highVisStyle.fontSize = 16; 
                _highVisStyle.fontStyle = FontStyle.Bold;
                _highVisStyle.normal.textColor = Color.black;
            }
        }
        
        public void ManagedOnDrawGizmos()
        {
            
        }

        private void ProcessLayChickenEggEvents(FixedUnorderedList<LayChickenEggNetEventS2C> layChickenEggNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedLayChickenEggEvents.Clear();
            foreach (var netEvent in layChickenEggNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedLayChickenEggEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedLayChickenEggEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedLayChickenEggEvents.Sort();
                _presentationNetEventsHandler.ProcessLayChickenEggEvents(_cachedUnprocessedLayChickenEggEvents);
            }
        }

        private void ProcessChickenEggHitEvents(FixedUnorderedList<ChickenEggHitNetEventS2C> chickenEggHitNetEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedChickenEggHitEvents.Clear();
            foreach (var netEvent in chickenEggHitNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedChickenEggHitEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedChickenEggHitEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedChickenEggHitEvents.Sort();
                _presentationNetEventsHandler.ProcessChickenEggHitEvents(_cachedUnprocessedChickenEggHitEvents);
            }
        }

        private void ProcessPerformGalacticPullNetEvents(FixedUnorderedList<PerformGalacticPullNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPerformGalacticPullNetEvents.Clear();
            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPerformGalacticPullNetEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedPerformGalacticPullNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPerformGalacticPullNetEvents.Sort();
                _presentationNetEventsHandler.ProcessPerformGalacticPullEvents(_cachedUnprocessedPerformGalacticPullNetEvents);
            }
        }

        private void ProcessDeactivateGalacticForceFieldNetEvents(FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateGalacticForceFieldNetEvents.Clear();
            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateGalacticForceFieldNetEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDeactivateGalacticForceFieldNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateGalacticForceFieldNetEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateGalacticForceFieldEvents(_cachedUnprocessedDeactivateGalacticForceFieldNetEvents);
            }
        }

        private void ProcessActivateNukePowerUpNetEvents(FixedUnorderedList<ActivateNukePowerUpNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateNukePowerUpNetEvents.Clear();
            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateNukePowerUpNetEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedActivateNukePowerUpNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateNukePowerUpNetEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateNukePowerUpEvents(_cachedUnprocessedActivateNukePowerUpNetEvents);
            }
        }

        private void ProcessActivateShufflePowerUpNetEvents(FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateShufflePowerUpNetEvents.Clear();
            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                    _cachedUnprocessedDeactivateShufflePowerUpNetEvents.Add(netEvent);
            }
            if (!_cachedUnprocessedDeactivateShufflePowerUpNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateShufflePowerUpNetEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateShufflePowerUpEvents(_cachedUnprocessedDeactivateShufflePowerUpNetEvents);
            }
        }

        private void ProcessShuffleSwapPlayerPositionNetEvents(FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedShuffleSwapPlayerPositionNetEvents.Clear();
            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                    _cachedUnprocessedShuffleSwapPlayerPositionNetEvents.Add(netEvent);
            }
            if (!_cachedUnprocessedShuffleSwapPlayerPositionNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedShuffleSwapPlayerPositionNetEvents.Sort();
                _presentationNetEventsHandler.ProcessShuffleSwapPlayerPositionEvents(_cachedUnprocessedShuffleSwapPlayerPositionNetEvents);
            }
        }

        private void ProcessActivateShuffleNetEvents(FixedUnorderedList<ActivateShuffleNetEventS2C> events, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateShuffleNetEvents.Clear();
            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                    _cachedUnprocessedActivateShuffleNetEvents.Add(netEvent);
            }
            if (!_cachedUnprocessedActivateShuffleNetEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateShuffleNetEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateShuffleEvents(_cachedUnprocessedActivateShuffleNetEvents);
            }
        }

        private void ProcessActivateHeadbuttChargingEvents(FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C> netEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedActivateHeadbuttChargingEvents.Clear();
            foreach (var netEvent in netEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedActivateHeadbuttChargingEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedActivateHeadbuttChargingEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedActivateHeadbuttChargingEvents.Sort();
                _presentationNetEventsHandler.ProcessActivateHeadbuttChargingEvents(_cachedUnprocessedActivateHeadbuttChargingEvents);
            }
        }

        private void ProcessPerformHeadbuttDashEvents(FixedUnorderedList<PerformHeadbuttDashNetEventS2C> netEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPerformHeadbuttDashEvents.Clear();
            foreach (var netEvent in netEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPerformHeadbuttDashEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedPerformHeadbuttDashEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPerformHeadbuttDashEvents.Sort();
                _presentationNetEventsHandler.ProcessPerformHeadbuttDashEvents(_cachedUnprocessedPerformHeadbuttDashEvents);
            }
        }

        private void ProcessPerformBarrelDashEvents(FixedUnorderedList<PerformBarrelDashNetEventS2C> netEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPerformBarrelDashEvents.Clear();
            foreach (var netEvent in netEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPerformBarrelDashEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedPerformBarrelDashEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPerformBarrelDashEvents.Sort();
                _presentationNetEventsHandler.ProcessPerformBarrelDashEvents(_cachedUnprocessedPerformBarrelDashEvents);
            }
        }

        private void ProcessPlayerSetMoveDestinationPointEvents(FixedUnorderedList<PlayerSetMoveDestinationPointNetEventS2C> netEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedPlayerSetMoveDestinationPointEvents.Clear();
            foreach (var netEvent in netEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedPlayerSetMoveDestinationPointEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedPlayerSetMoveDestinationPointEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerSetMoveDestinationPointEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerSetMoveDestinationPointEvents(_cachedUnprocessedPlayerSetMoveDestinationPointEvents);
            }
        }

        private void ProcessHeadbuttHitEnemyEvents(FixedUnorderedList<HeadbuttHitEnemyNetEventS2C> netEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedHeadbuttHitEnemyEvents.Clear();
            foreach (var netEvent in netEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedHeadbuttHitEnemyEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedHeadbuttHitEnemyEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedHeadbuttHitEnemyEvents.Sort();
                _presentationNetEventsHandler.ProcessHeadbuttHitEnemyEvents(_cachedUnprocessedHeadbuttHitEnemyEvents);
            }
        }

        private void ProcessDeactivateHeadbuttTalentEvents(FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C> netEvents, int ignoreEventsNotAboveTick)
        {
            _cachedUnprocessedDeactivateHeadbuttTalentEvents.Clear();
            foreach (var netEvent in netEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > ignoreEventsNotAboveTick)
                {
                    _cachedUnprocessedDeactivateHeadbuttTalentEvents.Add(netEvent);
                }
            }
            if (!_cachedUnprocessedDeactivateHeadbuttTalentEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedDeactivateHeadbuttTalentEvents.Sort();
                _presentationNetEventsHandler.ProcessDeactivateHeadbuttTalentEvents(_cachedUnprocessedDeactivateHeadbuttTalentEvents);
            }
        }
    }
}
