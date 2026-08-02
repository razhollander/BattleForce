using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class MatchFullTickPacketS2C : INetSerializable
    {
        public int Tick;
        //public SimulationStateS2C PreviousSimulationState; // not sure if gonna need this
        public MatchSimulationStateS2C CurrentSimulationState;
        public FixedUnorderedList<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed
        public FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C> PlayerJoinAcceptNetEvents;
        public FixedUnorderedList<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents;
        public FixedUnorderedList<PlayerDiedNetEventS2C> PlayerDiedNetEvents;
        public FixedUnorderedList<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents;
        public FixedUnorderedList<PlayersSwapNetEventS2C> PlayerSwapNetEvents;
        public FixedClassUnorderedList<TalentCardObtainedNetEventS2C> TalentCardObtainedNetEvents; // todo: remove events related to card when bullet id destroyed
        public FixedUnorderedList<TalentCardHitNetEventS2C> TalentCardHitNetEvents;
        public FixedUnorderedList<PlayerSpinnedStartedNetEventS2C> PlayerSpinnedStartedNetEvents;
        public FixedUnorderedList<PlayerSpinnedEndedNetEventS2C> PlayerSpinnedEndedNetEvents;
        public FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C> PlayerStartedExposedToLavaNetEvents;
        public FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C> PlayerEndedExposedToLavaNetEvents;
        public FixedUnorderedList<PowerUpBallSpawnedNetEventS2C> PowerUpSpawnedNetEvents; // todo: remove events related to power up when bullet id destroyed
        public FixedUnorderedList<PowerUpBallObtainedNetEventS2C> PowerUpObtainedNetEvents;
        public FixedClassUnorderedList<StageEndNetEventS2C> StageEndNetEvents;
        public FixedUnorderedList<TeamLostNetEventS2C> TeamLostNetEvents;
        public FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C> EnvironmentSpringPlayerCollisionNetEvents;
        public FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C> EnvironmentSpikePlayerCollisionNetEvents;
        public FixedUnorderedList<TalentSwitchNetEventS2C> TalentSwitchNetEvents;
        public FixedUnorderedList<GainBoltsNetEventS2C> GainBoltsNetEvents;
        public FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> PlayerToEnvironmentTeleportGateCollisionNetEvents;
        public FixedUnorderedList<PreparationPhaseEndedNetEventS2C> PreparationPhaseEndedNetEvents;
        public FixedUnorderedList<CreateSwapFieldNetEventS2C> CreateSwapFieldNetEvents;
        public FixedUnorderedList<CreateKOProjectileNetEventS2C> CreateKOProjectileNetEvents;
        public FixedUnorderedList<KOProjectHitPlayerNetEventS2C> KOProjectHitPlayerNetEvents;
        public FixedUnorderedList<DeactivateKOTalentNetEventS2C> DeactivateKOTalentNetEvents;
        public FixedUnorderedList<PerformDashPulseNetEventS2C> PerformDashPulseNetEvents;
        public FixedUnorderedList<ActivateSentryGunTalentNetEventS2C> ActivateSentryGunTalentNetEvents;
        public FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C> DeactivateSentryGunTalentNetEvents;
        public FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C> UpdatePlayerTalentStocksNetEvents;
        public FixedUnorderedList<DeactivateSwapTalentNetEventS2C> DestroySwapFieldNetEvents;
        public FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C> PlayerMaxShootCooldownChangedNetEvents;
        public FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C> CreateGrapplingHookProjectileNetEvents;
        public FixedUnorderedList<GrapplingHookHitWallNetEventS2C> GrapplingHookHitWallNetEvents;
        public FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C> DeactivateGrapplingHookTalentNetEvents;
        public FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C> ActivateUmbrellaTalentNetEvents;
        public FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C> DeactivateUmbrellaTalentNetEvents;
        public FixedUnorderedList<ActivateWaterGunTalentNetEventS2C> ActivateWaterGunTalentNetEvents;
        public FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C> DeactivateWaterGunTalentNetEvents;
        public FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C> ActivateHeadbuttChargingNetEvents;
        public FixedUnorderedList<PerformHeadbuttDashNetEventS2C> PerformHeadbuttDashNetEvents;
        public FixedUnorderedList<HeadbuttHitEnemyNetEventS2C> HeadbuttHitEnemyNetEvents;
        public FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C> DeactivateHeadbuttTalentNetEvents;
        public FixedUnorderedList<CreateMagneticPullFieldNetEventS2C> CreateMagneticPullFieldNetEvents;
        public FixedUnorderedList<LayChickenEggNetEventS2C> LayChickenEggNetEvents;
        public FixedUnorderedList<ChickenEggHitNetEventS2C> ChickenEggHitNetEvents;
        public FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C> ActivateYearsOfPainTalentNetEvents;
        public FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C> PlayerLockOnTargetsChangedNetEvents;
        public FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C> PlayerLockedOnTargetHitNetEvents;
        public FixedUnorderedList<PlayerPowerUpChangedNetEventS2C> PlayerPowerUpChangedNetEvents;
        public FixedClassUnorderedList<ActivateSonicSlapNetEventS2C> ActivateSonicSlapNetEvents;
        public FixedUnorderedList<PerformGalacticPullNetEventS2C> PerformGalacticPullNetEvents;
        public FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C> DeactivateGalacticForceFieldNetEvents;
        public FixedUnorderedList<ActivateNukePowerUpNetEventS2C> ActivateNukePowerUpNetEvents;
        public FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C> DeactivateShufflePowerUpNetEvents;
        public FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C> ShuffleSwapPlayerPositionNetEvents;
        public FixedUnorderedList<ActivateShuffleNetEventS2C> ActivateShuffleNetEvents;
        public FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C> StartPowerUpGrantingPhaseNetEvents;
        public FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C> EndPowerUpGrantingPhaseNetEvents;
        public FixedUnorderedList<ShootFrigidBlockNetEventS2C> ShootFrigidBlockNetEvents;
        public FixedUnorderedList<DestroyFrigidBlockNetEventS2C> DestroyFrigidBlockNetEvents;
        public FixedUnorderedList<CreateFishingRodProjectileNetEventS2C> CreateFishingRodProjectileNetEvents;
        public FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C> FishingRodCaughtEnemyNetEvents;
        public FixedUnorderedList<FishingRodTipHitWallNetEventS2C> FishingRodTipHitWallNetEvents;
        public FixedUnorderedList<FishingRodThrowNetEventS2C> FishingRodThrowNetEvents;
        public FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C> DeactivateFishingRodTalentNetEvents;
        public FixedUnorderedList<CreateSoulGhostNetEventS2C> CreateSoulGhostNetEvents;
        public FixedUnorderedList<DeactivateSoulTalentNetEventS2C> DeactivateSoulTalentNetEvents;
        public FixedUnorderedList<ActivateRockTalentNetEventS2C> ActivateRockTalentNetEvents;
        public FixedUnorderedList<DeactivateRockTalentNetEventS2C> DeactivateRockTalentNetEvents;
        public FixedUnorderedList<ActivateFrozenTalentNetEventS2C> ActivateFrozenTalentNetEvents;
        public FixedUnorderedList<DeactivateFrozenTalentNetEventS2C> DeactivateFrozenTalentNetEvents;
        public FixedUnorderedList<MoleSpawnedNetEventS2C> MoleSpawnedNetEvents;
        public FixedUnorderedList<MoleHitNetEventS2C> MoleHitNetEvents;
        public FixedUnorderedList<MoleExpiredNetEventS2C> MoleExpiredNetEvents;

        public MatchFullTickPacketS2C()
        {
            // use this from the server?
        }

        public MatchFullTickPacketS2C(MaxCap maxCap, SharedGamePlayConfig sharedGamePlayConfig)
        {
            CurrentSimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer,
                maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, sharedGamePlayConfig.MaxTeamsAmount, maxCap.ConcurrentChickenEggs, maxCap.ConcurrentGalacticForceFields, maxCap.ConcurrentFrigidBlocks,
                maxCap.ConcurrentMoles);

            BulletSpawnNetEvents = new FixedUnorderedList<BulletSpawnNetEventS2C>(maxCap.BulletSpawnNetEvents);

            PlayerJoinAcceptNetEvents = new FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C>(maxCap.PlayerJoinAcceptNetEvents,
                () => new PlayerRejoinAcceptPacketS2C(maxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount));

            PlayerTakeDamageNetEvents = new FixedUnorderedList<PlayerTakeDamageNetEventS2C>(maxCap.PlayerTakeDamageNetEvents);
            PlayerDiedNetEvents = new FixedUnorderedList<PlayerDiedNetEventS2C>(maxCap.PlayerDiedNetEvents);
            BulletDestroyedNetEvents = new FixedUnorderedList<BulletDestroyedNetEventS2C>(maxCap.BulletDestroyedNetEvents);
            PlayerSwapNetEvents = new FixedUnorderedList<PlayersSwapNetEventS2C>(maxCap.PlayerSwapNetEvents);

            TalentCardObtainedNetEvents = new FixedClassUnorderedList<TalentCardObtainedNetEventS2C>(maxCap.TalentCardObtainedNetEvent,
                () => new TalentCardObtainedNetEventS2C(sharedGamePlayConfig.MaxConcurrentTalentsForPlayer));

            TalentCardHitNetEvents = new FixedUnorderedList<TalentCardHitNetEventS2C>(maxCap.TalentCardHitNetEvents);
            PlayerSpinnedStartedNetEvents = new FixedUnorderedList<PlayerSpinnedStartedNetEventS2C>(maxCap.PlayerSpinnedStartedNetEvents);
            PlayerSpinnedEndedNetEvents = new FixedUnorderedList<PlayerSpinnedEndedNetEventS2C>(maxCap.PlayerSpinnedEndedNetEvents);
            PlayerStartedExposedToLavaNetEvents = new FixedUnorderedList<PlayerStartedExposedToLavaNetEventS2C>(maxCap.PlayerStartedExposedToLavaNetEvents);
            PlayerEndedExposedToLavaNetEvents = new FixedUnorderedList<PlayerEndedExposedToLavaNetEventS2C>(maxCap.PlayerEndedExposedToLavaNetEvents);
            PowerUpSpawnedNetEvents = new FixedUnorderedList<PowerUpBallSpawnedNetEventS2C>(maxCap.PowerUpSpawnedNetEvents);
            PowerUpObtainedNetEvents = new FixedUnorderedList<PowerUpBallObtainedNetEventS2C>(maxCap.PowerUpObtainedNetEvents);
            StageEndNetEvents = new FixedClassUnorderedList<StageEndNetEventS2C>(maxCap.StageEndNetEvents, () => new StageEndNetEventS2C(sharedGamePlayConfig.MaxTeamsAmount));
            TeamLostNetEvents = new FixedUnorderedList<TeamLostNetEventS2C>(sharedGamePlayConfig.MaxTeamsAmount);
            TalentSwitchNetEvents = new FixedUnorderedList<TalentSwitchNetEventS2C>(maxCap.TalentSwitchNetEvents);
            EnvironmentSpringPlayerCollisionNetEvents = new FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C>(maxCap.EnvironmentSpringPlayerCollisionNetEvents);
            EnvironmentSpikePlayerCollisionNetEvents = new FixedUnorderedList<EnvironmentSpikePlayerCollisionNetEventS2C>(maxCap.EnvironmentSpikePlayerCollisionNetEvents);
            GainBoltsNetEvents = new FixedUnorderedList<GainBoltsNetEventS2C>(maxCap.GainBoltsNetEvents);

            PlayerToEnvironmentTeleportGateCollisionNetEvents =
                new FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>(maxCap.PlayerToEnvironmentTeleportGateCollisionNetEvents);

            PreparationPhaseEndedNetEvents = new FixedUnorderedList<PreparationPhaseEndedNetEventS2C>(maxCap.PreparationPhaseEndedNetEvents);
            CreateSwapFieldNetEvents = new FixedUnorderedList<CreateSwapFieldNetEventS2C>(maxCap.CreateSwapFieldNetEvents);
            DestroySwapFieldNetEvents = new FixedUnorderedList<DeactivateSwapTalentNetEventS2C>(maxCap.DestroySwapFieldNetEvents);
            KOProjectHitPlayerNetEvents = new FixedUnorderedList<KOProjectHitPlayerNetEventS2C>(maxCap.KOProjectHitPlayerNetEvents);
            CreateKOProjectileNetEvents = new FixedUnorderedList<CreateKOProjectileNetEventS2C>(maxCap.CreateKOProjectileNetEvents);
            DeactivateKOTalentNetEvents = new FixedUnorderedList<DeactivateKOTalentNetEventS2C>(maxCap.DeactivateKOTalentNetEvents);
            PerformDashPulseNetEvents = new FixedUnorderedList<PerformDashPulseNetEventS2C>(maxCap.PerformDashPulseNetEvents);
            ActivateSentryGunTalentNetEvents = new FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>(maxCap.ActivateSentryGunTalentNetEvents);
            DeactivateSentryGunTalentNetEvents = new FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>(maxCap.DeactivateSentryGunTalentNetEvents);
            UpdatePlayerTalentStocksNetEvents = new FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>(maxCap.UpdatePlayerTalentStocksNetEvents);
            PlayerMaxShootCooldownChangedNetEvents = new FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>(maxCap.PlayerMaxShootCooldownChangedNetEvents);
            ActivateSentryGunTalentNetEvents = new FixedUnorderedList<ActivateSentryGunTalentNetEventS2C>(maxCap.ActivateSentryGunTalentNetEvents);
            DeactivateSentryGunTalentNetEvents = new FixedUnorderedList<DeactivateSentryGunTalentNetEventS2C>(maxCap.DeactivateSentryGunTalentNetEvents);
            CreateGrapplingHookProjectileNetEvents = new FixedUnorderedList<CreateGrapplingHookProjectileNetEventS2C>(maxCap.CreateGrapplingHookProjectileNetEvents);
            GrapplingHookHitWallNetEvents = new FixedUnorderedList<GrapplingHookHitWallNetEventS2C>(maxCap.GrapplingHookHitWallNetEvents);
            DeactivateGrapplingHookTalentNetEvents = new FixedUnorderedList<DeactivateGrapplingHookTalentNetEventS2C>(maxCap.DeactivateGrapplingHookTalentNetEvents);
            CreateMagneticPullFieldNetEvents = new FixedUnorderedList<CreateMagneticPullFieldNetEventS2C>(maxCap.CreateMagneticPullFieldNetEvents);
            ActivateYearsOfPainTalentNetEvents = new FixedUnorderedList<ActivateYearsOfPainTalentNetEventS2C>(maxCap.ActivateYearsOfPainTalentNetEvents);
            ActivateUmbrellaTalentNetEvents = new FixedUnorderedList<ActivateUmbrellaTalentNetEventS2C>(maxCap.ActivateUmbrellaTalentNetEvents);
            DeactivateUmbrellaTalentNetEvents = new FixedUnorderedList<DeactivateUmbrellaTalentNetEventS2C>(maxCap.DeactivateUmbrellaTalentNetEvents);
            ActivateWaterGunTalentNetEvents = new FixedUnorderedList<ActivateWaterGunTalentNetEventS2C>(maxCap.ActivateWaterGunTalentNetEvents);
            DeactivateWaterGunTalentNetEvents = new FixedUnorderedList<DeactivateWaterGunTalentNetEventS2C>(maxCap.DeactivateWaterGunTalentNetEvents);
            ActivateHeadbuttChargingNetEvents = new FixedUnorderedList<ActivateHeadbuttChargingNetEventS2C>(maxCap.ActivateHeadbuttChargingNetEvents);
            PerformHeadbuttDashNetEvents = new FixedUnorderedList<PerformHeadbuttDashNetEventS2C>(maxCap.PerformHeadbuttDashNetEvents);
            HeadbuttHitEnemyNetEvents = new FixedUnorderedList<HeadbuttHitEnemyNetEventS2C>(maxCap.HeadbuttHitEnemyNetEvents);
            DeactivateHeadbuttTalentNetEvents = new FixedUnorderedList<DeactivateHeadbuttTalentNetEventS2C>(maxCap.DeactivateHeadbuttTalentNetEvents);
            LayChickenEggNetEvents = new FixedUnorderedList<LayChickenEggNetEventS2C>(maxCap.LayChickenEggNetEvents);
            ChickenEggHitNetEvents = new FixedUnorderedList<ChickenEggHitNetEventS2C>(maxCap.ChickenEggHitNetEvents);
            PlayerLockOnTargetsChangedNetEvents = new FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>(maxCap.PlayerLockOnTargetsChangedNetEvents, () => new PlayerLockOnTargetsChangedNetEventS2C(maxCap.ConcurrentLockOnTargets));
            PlayerLockedOnTargetHitNetEvents = new FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>(maxCap.ConcurrentPlayers);
            PlayerPowerUpChangedNetEvents = new FixedUnorderedList<PlayerPowerUpChangedNetEventS2C>(maxCap.PlayerPowerUpChangedNetEvents);
            ActivateSonicSlapNetEvents = new FixedClassUnorderedList<ActivateSonicSlapNetEventS2C>(maxCap.ActivateSonicSlapNetEvents, () => new ActivateSonicSlapNetEventS2C(maxCap.ConcurrentEnemyPlayers));
            PerformGalacticPullNetEvents = new FixedUnorderedList<PerformGalacticPullNetEventS2C>(maxCap.PerformGalacticPullNetEvents);
            DeactivateGalacticForceFieldNetEvents = new FixedUnorderedList<DeactivateGalacticForceFieldNetEventS2C>(maxCap.DeactivateGalacticForceFieldNetEvents);
            ActivateNukePowerUpNetEvents = new FixedUnorderedList<ActivateNukePowerUpNetEventS2C>(maxCap.ActivateNukePowerUpNetEvents);
            DeactivateShufflePowerUpNetEvents = new FixedUnorderedList<DeactivateShufflePowerUpNetEventS2C>(maxCap.ActivateShufflePowerUpNetEvents);
            ShuffleSwapPlayerPositionNetEvents = new FixedUnorderedList<ShuffleSwapPlayerPositionNetEventS2C>(maxCap.ShuffleSwapPlayerPositionNetEvents);
            ActivateShuffleNetEvents = new FixedUnorderedList<ActivateShuffleNetEventS2C>(maxCap.ActivateShufflePowerUpNetEvents);
            StartPowerUpGrantingPhaseNetEvents = new FixedUnorderedList<StartPowerUpGrantingPhaseNetEventS2C>(maxCap.StartPowerUpGrantingPhaseNetEvents);
            EndPowerUpGrantingPhaseNetEvents = new FixedUnorderedList<EndPowerUpGrantingPhaseNetEventS2C>(maxCap.EndPowerUpGrantingPhaseNetEvents);
            ShootFrigidBlockNetEvents = new FixedUnorderedList<ShootFrigidBlockNetEventS2C>(maxCap.ShootFrigidBlockNetEvents);
            DestroyFrigidBlockNetEvents = new FixedUnorderedList<DestroyFrigidBlockNetEventS2C>(maxCap.DestroyFrigidBlockNetEvents);
            CreateFishingRodProjectileNetEvents = new FixedUnorderedList<CreateFishingRodProjectileNetEventS2C>(maxCap.CreateFishingRodProjectileNetEvents);
            FishingRodCaughtEnemyNetEvents = new FixedUnorderedList<FishingRodCaughtEnemyNetEventS2C>(maxCap.FishingRodCaughtEnemyNetEvents);
            FishingRodTipHitWallNetEvents = new FixedUnorderedList<FishingRodTipHitWallNetEventS2C>(maxCap.FishingRodTipHitWallNetEvents);
            FishingRodThrowNetEvents = new FixedUnorderedList<FishingRodThrowNetEventS2C>(maxCap.FishingRodThrowNetEvents);
            DeactivateFishingRodTalentNetEvents = new FixedUnorderedList<DeactivateFishingRodTalentNetEventS2C>(maxCap.DeactivateFishingRodTalentNetEvents);
            CreateSoulGhostNetEvents = new FixedUnorderedList<CreateSoulGhostNetEventS2C>(maxCap.CreateSoulGhostNetEvents);
            DeactivateSoulTalentNetEvents = new FixedUnorderedList<DeactivateSoulTalentNetEventS2C>(maxCap.DeactivateSoulTalentNetEvents);
            ActivateRockTalentNetEvents = new FixedUnorderedList<ActivateRockTalentNetEventS2C>(maxCap.ActivateRockTalentNetEvents);
            DeactivateRockTalentNetEvents = new FixedUnorderedList<DeactivateRockTalentNetEventS2C>(maxCap.DeactivateRockTalentNetEvents);
            ActivateFrozenTalentNetEvents = new FixedUnorderedList<ActivateFrozenTalentNetEventS2C>(maxCap.ActivateFrozenTalentNetEvents);
            DeactivateFrozenTalentNetEvents = new FixedUnorderedList<DeactivateFrozenTalentNetEventS2C>(maxCap.DeactivateFrozenTalentNetEvents);
            MoleSpawnedNetEvents = new FixedUnorderedList<MoleSpawnedNetEventS2C>(maxCap.MoleSpawnedNetEvents);
            MoleHitNetEvents = new FixedUnorderedList<MoleHitNetEventS2C>(maxCap.MoleHitNetEvents);
            MoleExpiredNetEvents = new FixedUnorderedList<MoleExpiredNetEventS2C>(maxCap.MoleExpiredNetEvents);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            CurrentSimulationState.SerializeDeltas(writer);
            
            var eventMask = CalculateEventMask();
            writer.Put(eventMask);

            // The primary 64-bit mask is fully used (bits 0-63). eventMask2 carries overflow events (bit 64+).
            var eventMask2 = CalculateEventMask2();
            writer.Put(eventMask2);

            if ((eventMask & (1UL << 0)) != 0) SerializedPlayerJoinedEvents(writer);
            if ((eventMask & (1UL << 1)) != 0) SerializedBulletSpawnedEvents(writer);
            if ((eventMask & (1UL << 2)) != 0) SerializedPlayerTakeDamageEvents(writer);
            if ((eventMask & (1UL << 3)) != 0) SerializedPlayerDiedEvents(writer);
            if ((eventMask & (1UL << 4)) != 0) SerializedPlayerLockOnTargetsChangedNetEvents(writer);
            if ((eventMask & (1UL << 5)) != 0) SerializedPlayerLockedOnTargetHitNetEvents(writer);
            if ((eventMask & (1UL << 6)) != 0) SerializedBulletDestroyedEvents(writer);
            if ((eventMask & (1UL << 7)) != 0) SerializedPlayerSwapEvents(writer);
            if ((eventMask & (1UL << 8)) != 0) SerializedTalentCardObtainedEvents(writer);
            if ((eventMask & (1UL << 9)) != 0) SerializedTalentCardHitEvents(writer);
            if ((eventMask & (1UL << 10)) != 0) SerializedPowerUpSpawnedEvents(writer);
            if ((eventMask & (1UL << 11)) != 0) SerializedPowerUpObtainedEvents(writer);
            if ((eventMask & (1UL << 12)) != 0) SerializedStageEndEvents(writer);
            if ((eventMask & (1UL << 13)) != 0) SerializedTeamLostEvents(writer);
            if ((eventMask & (1UL << 14)) != 0) SerializedTalentSwitchEvents(writer);
            if ((eventMask & (1UL << 15)) != 0) SerializedEnvironmentSpringPlayerCollisionEvents(writer);
            if ((eventMask & (1UL << 16)) != 0) SerializedGainBoltsEvents(writer);
            if ((eventMask & (1UL << 17)) != 0) SerializedPlayerToEnvironmentTeleportGateCollisionEvents(writer);
            if ((eventMask & (1UL << 18)) != 0) SerializedPreparationPhaseEndedEvents(writer);
            if ((eventMask & (1UL << 19)) != 0) SerializedCreateSwapFieldNetEvents(writer);
            if ((eventMask & (1UL << 20)) != 0) SerializedCreateKOProjectileNetEvents(writer);
            if ((eventMask & (1UL << 21)) != 0) SerializedKOProjectHitPlayerNetEvents(writer);
            if ((eventMask & (1UL << 22)) != 0) SerializedDeactivateKOTalentNetEvents(writer);
            if ((eventMask & (1UL << 23)) != 0) SerializedPerformDashPulseNetEvents(writer);
            if ((eventMask & (1UL << 24)) != 0) SerializedActivateSentryGunTalentNetEvents(writer);
            if ((eventMask & (1UL << 25)) != 0) SerializedDeactivateSentryGunTalentNetEvents(writer);
            if ((eventMask & (1UL << 26)) != 0) SerializedUpdatePlayerTalentStocksNetEvents(writer);
            if ((eventMask & (1UL << 27)) != 0) SerializedPlayerSpinnedStartedEvents(writer);
            if ((eventMask & (1UL << 28)) != 0) SerializedPlayerSpinnedEndedEvents(writer);
            if ((eventMask & (1UL << 29)) != 0) SerializedDestroySwapFieldNetEvents(writer);
            if ((eventMask & (1UL << 30)) != 0) SerializedPlayerMaxShootCooldownChangedNetEvents(writer);
            if ((eventMask & (1UL << 31)) != 0) SerializedCreateGrapplingHookProjectileNetEvents(writer);
            if ((eventMask & (1UL << 32)) != 0) SerializedGrapplingHookHitWallNetEvents(writer);
            if ((eventMask & (1UL << 33)) != 0) SerializedDeactivateGrapplingHookTalentNetEvents(writer);
            if ((eventMask & (1UL << 34)) != 0) SerializedCreateMagneticPullFieldNetEvents(writer);
            if ((eventMask & (1UL << 35)) != 0) SerializedActivateUmbrellaTalentNetEvents(writer);
            if ((eventMask & (1UL << 36)) != 0) SerializedDeactivateUmbrellaTalentNetEvents(writer);
            if ((eventMask & (1UL << 37)) != 0) SerializedLayChickenEggNetEvents(writer);
            if ((eventMask & (1UL << 38)) != 0) SerializedChickenEggHitNetEvents(writer);
            if ((eventMask & (1UL << 39)) != 0) SerializedActivateYearsOfPainTalentNetEvents(writer);
            if ((eventMask & (1UL << 42)) != 0) SerializedEnvironmentSpikePlayerCollisionEvents(writer);
            if ((eventMask & (1UL << 40)) != 0) SerializedPlayerPowerUpChangedNetEvents(writer);
            if ((eventMask & (1UL << 41)) != 0) SerializedActivateSonicSlapNetEvents(writer);
            if ((eventMask & (1UL << 43)) != 0) SerializedPerformGalacticPullNetEvents(writer);
            if ((eventMask & (1UL << 44)) != 0) SerializedDeactivateGalacticForceFieldNetEvents(writer);
            if ((eventMask & (1UL << 45)) != 0) SerializedActivateNukePowerUpNetEvents(writer);
            if ((eventMask & (1UL << 46)) != 0) SerializedDeactivateShufflePowerUpNetEvents(writer);
            if ((eventMask & (1UL << 47)) != 0) SerializedShuffleSwapPlayerPositionNetEvents(writer);
            if ((eventMask & (1UL << 48)) != 0) SerializedActivateShuffleNetEvents(writer);
            if ((eventMask & (1UL << 49)) != 0) SerializedStartPowerUpGrantingPhaseNetEvents(writer);
            if ((eventMask & (1UL << 50)) != 0) SerializedEndPowerUpGrantingPhaseNetEvents(writer);
            if ((eventMask & (1UL << 51)) != 0) SerializedActivateWaterGunTalentNetEvents(writer);
            if ((eventMask & (1UL << 52)) != 0) SerializedDeactivateWaterGunTalentNetEvents(writer);
            if ((eventMask & (1UL << 53)) != 0) SerializedActivateHeadbuttChargingNetEvents(writer);
            if ((eventMask & (1UL << 54)) != 0) SerializedPerformHeadbuttDashNetEvents(writer);
            if ((eventMask & (1UL << 55)) != 0) SerializedHeadbuttHitEnemyNetEvents(writer);
            if ((eventMask & (1UL << 56)) != 0) SerializedDeactivateHeadbuttTalentNetEvents(writer);
            if ((eventMask & (1UL << 57)) != 0) SerializedShootFrigidBlockNetEvents(writer);
            if ((eventMask & (1UL << 58)) != 0) SerializedDestroyFrigidBlockNetEvents(writer);
            if ((eventMask & (1UL << 59)) != 0) SerializedCreateFishingRodProjectileNetEvents(writer);
            if ((eventMask & (1UL << 60)) != 0) SerializedFishingRodCaughtEnemyNetEvents(writer);
            if ((eventMask & (1UL << 61)) != 0) SerializedFishingRodTipHitWallNetEvents(writer);
            if ((eventMask & (1UL << 62)) != 0) SerializedFishingRodThrowNetEvents(writer);
            if ((eventMask & (1UL << 63)) != 0) SerializedDeactivateFishingRodTalentNetEvents(writer);

            if ((eventMask2 & (1UL << 0)) != 0) SerializedCreateSoulGhostNetEvents(writer);
            if ((eventMask2 & (1UL << 1)) != 0) SerializedDeactivateSoulTalentNetEvents(writer);
            if ((eventMask2 & (1UL << 2)) != 0) SerializedActivateRockTalentNetEvents(writer);
            if ((eventMask2 & (1UL << 3)) != 0) SerializedDeactivateRockTalentNetEvents(writer);
            if ((eventMask2 & (1UL << 4)) != 0) SerializedPlayerStartedExposedToLavaNetEvents(writer);
            if ((eventMask2 & (1UL << 5)) != 0) SerializedPlayerEndedExposedToLavaNetEvents(writer);
            if ((eventMask2 & (1UL << 6)) != 0) SerializedActivateFrozenTalentNetEvents(writer);
            if ((eventMask2 & (1UL << 7)) != 0) SerializedDeactivateFrozenTalentNetEvents(writer);
            if ((eventMask2 & (1UL << 8)) != 0) SerializedMoleSpawnedNetEvents(writer);
            if ((eventMask2 & (1UL << 9)) != 0) SerializedMoleHitNetEvents(writer);
            if ((eventMask2 & (1UL << 10)) != 0) SerializedMoleExpiredNetEvents(writer);
        }

        private ulong CalculateEventMask2()
        {
            ulong eventMask2 = 0;
            if (CreateSoulGhostNetEvents.Count > 0) eventMask2 |= 1UL << 0;
            if (DeactivateSoulTalentNetEvents.Count > 0) eventMask2 |= 1UL << 1;
            if (ActivateRockTalentNetEvents.Count > 0) eventMask2 |= 1UL << 2;
            if (DeactivateRockTalentNetEvents.Count > 0) eventMask2 |= 1UL << 3;
            if (PlayerStartedExposedToLavaNetEvents.Count > 0) eventMask2 |= 1UL << 4;
            if (PlayerEndedExposedToLavaNetEvents.Count > 0) eventMask2 |= 1UL << 5;
            if (ActivateFrozenTalentNetEvents.Count > 0) eventMask2 |= 1UL << 6;
            if (DeactivateFrozenTalentNetEvents.Count > 0) eventMask2 |= 1UL << 7;
            if (MoleSpawnedNetEvents.Count > 0) eventMask2 |= 1UL << 8;
            if (MoleHitNetEvents.Count > 0) eventMask2 |= 1UL << 9;
            if (MoleExpiredNetEvents.Count > 0) eventMask2 |= 1UL << 10;
            return eventMask2;
        }

        private ulong CalculateEventMask()
        {
            ulong eventMask = 0;
            if (PlayerJoinAcceptNetEvents.Count > 0) eventMask |= 1UL << 0;
            if (BulletSpawnNetEvents.Count > 0) eventMask |= 1UL << 1;
            if (PlayerTakeDamageNetEvents.Count > 0) eventMask |= 1UL << 2;
            if (PlayerDiedNetEvents.Count > 0) eventMask |= 1UL << 3;
            if (PlayerLockOnTargetsChangedNetEvents.Count > 0) eventMask |= 1UL << 4;
            if (PlayerLockedOnTargetHitNetEvents.Count > 0) eventMask |= 1UL << 5;
            if (BulletDestroyedNetEvents.Count > 0) eventMask |= 1UL << 6;
            if (PlayerSwapNetEvents.Count > 0) eventMask |= 1UL << 7;
            if (TalentCardObtainedNetEvents.Count > 0) eventMask |= 1UL << 8;
            if (TalentCardHitNetEvents.Count > 0) eventMask |= 1UL << 9;
            if (PowerUpSpawnedNetEvents.Count > 0) eventMask |= 1UL << 10;
            if (PowerUpObtainedNetEvents.Count > 0) eventMask |= 1UL << 11;
            if (StageEndNetEvents.Count > 0) eventMask |= 1UL << 12;
            if (TeamLostNetEvents.Count > 0) eventMask |= 1UL << 13;
            if (TalentSwitchNetEvents.Count > 0) eventMask |= 1UL << 14;
            if (EnvironmentSpringPlayerCollisionNetEvents.Count > 0) eventMask |= 1UL << 15;
            if (GainBoltsNetEvents.Count > 0) eventMask |= 1UL << 16;
            if (PlayerToEnvironmentTeleportGateCollisionNetEvents.Count > 0) eventMask |= 1UL << 17;
            if (PreparationPhaseEndedNetEvents.Count > 0) eventMask |= 1UL << 18;
            if (CreateSwapFieldNetEvents.Count > 0) eventMask |= 1UL << 19;
            if (CreateKOProjectileNetEvents.Count > 0) eventMask |= 1UL << 20;
            if (KOProjectHitPlayerNetEvents.Count > 0) eventMask |= 1UL << 21;
            if (DeactivateKOTalentNetEvents.Count > 0) eventMask |= 1UL << 22;
            if (PerformDashPulseNetEvents.Count > 0) eventMask |= 1UL << 23;
            if (ActivateSentryGunTalentNetEvents.Count > 0) eventMask |= 1UL << 24;
            if (DeactivateSentryGunTalentNetEvents.Count > 0) eventMask |= 1UL << 25;
            if (UpdatePlayerTalentStocksNetEvents.Count > 0) eventMask |= 1UL << 26;
            if (PlayerSpinnedStartedNetEvents.Count > 0) eventMask |= 1UL << 27;
            if (PlayerSpinnedEndedNetEvents.Count > 0) eventMask |= 1UL << 28;
            if (DestroySwapFieldNetEvents.Count > 0) eventMask |= 1UL << 29;
            if (PlayerMaxShootCooldownChangedNetEvents.Count > 0) eventMask |= 1UL << 30;
            if (CreateGrapplingHookProjectileNetEvents.Count > 0) eventMask |= 1UL << 31;
            if (GrapplingHookHitWallNetEvents.Count > 0) eventMask |= 1UL << 32;
            if (DeactivateGrapplingHookTalentNetEvents.Count > 0) eventMask |= 1UL << 33;
            if (CreateMagneticPullFieldNetEvents.Count > 0) eventMask |= 1UL << 34;
            if (ActivateUmbrellaTalentNetEvents.Count > 0) eventMask |= 1UL << 35;
            if (DeactivateUmbrellaTalentNetEvents.Count > 0) eventMask |= 1UL << 36;
            if (LayChickenEggNetEvents.Count > 0) eventMask |= 1UL << 37;
            if (ChickenEggHitNetEvents.Count > 0) eventMask |= 1UL << 38;
            if (ActivateYearsOfPainTalentNetEvents.Count > 0) eventMask |= 1UL << 39;
            if (EnvironmentSpikePlayerCollisionNetEvents.Count > 0) eventMask |= 1UL << 42;
            if (PlayerPowerUpChangedNetEvents.Count > 0) eventMask |= 1UL << 40;
            if (ActivateSonicSlapNetEvents.Count > 0) eventMask |= 1UL << 41;
            if (PerformGalacticPullNetEvents.Count > 0) eventMask |= 1UL << 43;
            if (DeactivateGalacticForceFieldNetEvents.Count > 0) eventMask |= 1UL << 44;
            if (ActivateNukePowerUpNetEvents.Count > 0) eventMask |= 1UL << 45;
            if (DeactivateShufflePowerUpNetEvents.Count > 0) eventMask |= 1UL << 46;
            if (ShuffleSwapPlayerPositionNetEvents.Count > 0) eventMask |= 1UL << 47;
            if (ActivateShuffleNetEvents.Count > 0) eventMask |= 1UL << 48;
            if (StartPowerUpGrantingPhaseNetEvents.Count > 0) eventMask |= 1UL << 49;
            if (EndPowerUpGrantingPhaseNetEvents.Count > 0) eventMask |= 1UL << 50;
            if (ActivateWaterGunTalentNetEvents.Count > 0) eventMask |= 1UL << 51;
            if (DeactivateWaterGunTalentNetEvents.Count > 0) eventMask |= 1UL << 52;
            if (ActivateHeadbuttChargingNetEvents.Count > 0) eventMask |= 1UL << 53;
            if (PerformHeadbuttDashNetEvents.Count > 0) eventMask |= 1UL << 54;
            if (HeadbuttHitEnemyNetEvents.Count > 0) eventMask |= 1UL << 55;
            if (DeactivateHeadbuttTalentNetEvents.Count > 0) eventMask |= 1UL << 56;
            if (ShootFrigidBlockNetEvents.Count > 0) eventMask |= 1UL << 57;
            if (DestroyFrigidBlockNetEvents.Count > 0) eventMask |= 1UL << 58;
            if (CreateFishingRodProjectileNetEvents.Count > 0) eventMask |= 1UL << 59;
            if (FishingRodCaughtEnemyNetEvents.Count > 0) eventMask |= 1UL << 60;
            if (FishingRodTipHitWallNetEvents.Count > 0) eventMask |= 1UL << 61;
            if (FishingRodThrowNetEvents.Count > 0) eventMask |= 1UL << 62;
            if (DeactivateFishingRodTalentNetEvents.Count > 0) eventMask |= 1UL << 63;
            return eventMask;
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            CurrentSimulationState.DeserializeTransforms(reader);
            
            ulong eventMask = reader.GetULong();
            ulong eventMask2 = reader.GetULong();

            if ((eventMask & (1UL << 0)) != 0) DeserializedPlayerJoinedEvents(reader);
            else PlayerJoinAcceptNetEvents.Clear();

            if ((eventMask & (1UL << 1)) != 0) DeserializedBulletSpawnedEvents(reader);
            else BulletSpawnNetEvents.Clear();

            if ((eventMask & (1UL << 2)) != 0) DeserializedPlayerTakeDamageEvents(reader);
            else PlayerTakeDamageNetEvents.Clear();

            if ((eventMask & (1UL << 3)) != 0) DeserializedPlayerDiedEvents(reader);
            else PlayerDiedNetEvents.Clear();

            if ((eventMask & (1UL << 4)) != 0) DeserializedPlayerLockOnTargetsChangedNetEvents(reader);
            else
            {
                for (int i = 0; i < PlayerLockOnTargetsChangedNetEvents.Count; i++)
                    PlayerLockOnTargetsChangedNetEvents[i].LockedOnTargetObjects.Clear();
                PlayerLockOnTargetsChangedNetEvents.Clear();
            }
            
            if ((eventMask & (1UL << 5)) != 0) DeserializedPlayerLockedOnTargetHitNetEvents(reader);
            else PlayerLockedOnTargetHitNetEvents.Clear();

            if ((eventMask & (1UL << 6)) != 0) DeserializedBulletDestroyedEvents(reader);
            else BulletDestroyedNetEvents.Clear();

            if ((eventMask & (1UL << 7)) != 0) DeserializedPlayerSwapEvents(reader);
            else PlayerSwapNetEvents.Clear();

            if ((eventMask & (1UL << 8)) != 0) DeserializedTalentCardObtainedEvents(reader);
            else TalentCardObtainedNetEvents.Clear();

            if ((eventMask & (1UL << 9)) != 0) DeserializedTalentCardHitEvents(reader);
            else TalentCardHitNetEvents.Clear();

            if ((eventMask & (1UL << 10)) != 0) DeserializedPowerUpSpawnedEvents(reader);
            else PowerUpSpawnedNetEvents.Clear();

            if ((eventMask & (1UL << 11)) != 0) DeserializedPowerUpObtainedEvents(reader);
            else PowerUpObtainedNetEvents.Clear();

            if ((eventMask & (1UL << 12)) != 0) DeserializedStageEndEvents(reader);
            else StageEndNetEvents.Clear();

            if ((eventMask & (1UL << 13)) != 0) DeserializedTeamLostEvents(reader);
            else TeamLostNetEvents.Clear();

            if ((eventMask & (1UL << 14)) != 0) DeserializedTalentSwitchEvents(reader);
            else TalentSwitchNetEvents.Clear();

            if ((eventMask & (1UL << 15)) != 0) DeserializedEnvironmentSpringPlayerCollisionEvents(reader);
            else EnvironmentSpringPlayerCollisionNetEvents.Clear();

            if ((eventMask & (1UL << 16)) != 0) DeserializedGainBoltsEvents(reader);
            else GainBoltsNetEvents.Clear();

            if ((eventMask & (1UL << 17)) != 0) DeserializedPlayerToEnvironmentTeleportGateCollisionEvents(reader);
            else PlayerToEnvironmentTeleportGateCollisionNetEvents.Clear();

            if ((eventMask & (1UL << 18)) != 0) DeserializedPreparationPhaseEndedEvents(reader);
            else PreparationPhaseEndedNetEvents.Clear();

            if ((eventMask & (1UL << 19)) != 0) DeserializedCreateSwapFieldNetEvents(reader);
            else CreateSwapFieldNetEvents.Clear();

            if ((eventMask & (1UL << 20)) != 0) DeserializedCreateKOProjectileNetEvents(reader);
            else CreateKOProjectileNetEvents.Clear();

            if ((eventMask & (1UL << 21)) != 0) DeserializedKOProjectHitPlayerNetEvents(reader);
            else KOProjectHitPlayerNetEvents.Clear();

            if ((eventMask & (1UL << 22)) != 0) DeserializedDeactivateKOTalentNetEvents(reader);
            else DeactivateKOTalentNetEvents.Clear();

            if ((eventMask & (1UL << 23)) != 0) DeserializedPerformDashPulseNetEvents(reader);
            else PerformDashPulseNetEvents.Clear();

            if ((eventMask & (1UL << 24)) != 0) DeserializedActivateSentryGunTalentNetEvents(reader);
            else ActivateSentryGunTalentNetEvents.Clear();

            if ((eventMask & (1UL << 25)) != 0) DeserializedDeactivateSentryGunTalentNetEvents(reader);
            else DeactivateSentryGunTalentNetEvents.Clear();

            if ((eventMask & (1UL << 26)) != 0) DeserializedUpdatePlayerTalentStocksNetEvents(reader);
            else UpdatePlayerTalentStocksNetEvents.Clear();

            if ((eventMask & (1UL << 27)) != 0) DeserializedPlayerSpinnedStartedEvents(reader);
            else PlayerSpinnedStartedNetEvents.Clear();

            if ((eventMask & (1UL << 28)) != 0) DeserializedPlayerSpinnedEndedEvents(reader);
            else PlayerSpinnedEndedNetEvents.Clear();

            if ((eventMask & (1UL << 29)) != 0) DeserializedDestroySwapFieldNetEvents(reader);
            else DestroySwapFieldNetEvents.Clear();

            if ((eventMask & (1UL << 30)) != 0) DeserializedPlayerMaxShootCooldownChangedNetEvents(reader);
            else PlayerMaxShootCooldownChangedNetEvents.Clear();

            if ((eventMask & (1UL << 31)) != 0) DeserializedCreateGrapplingHookProjectileNetEvents(reader);
            else CreateGrapplingHookProjectileNetEvents.Clear();

            if ((eventMask & (1UL << 32)) != 0) DeserializedGrapplingHookHitWallNetEvents(reader);
            else GrapplingHookHitWallNetEvents.Clear();

            if ((eventMask & (1UL << 33)) != 0) DeserializedDeactivateGrapplingHookTalentNetEvents(reader);
            else DeactivateGrapplingHookTalentNetEvents.Clear();

            if ((eventMask & (1UL << 34)) != 0) DeserializedCreateMagneticPullFieldNetEvents(reader);
            else CreateMagneticPullFieldNetEvents.Clear();

            if ((eventMask & (1UL << 35)) != 0) DeserializedActivateUmbrellaTalentNetEvents(reader);
            else ActivateUmbrellaTalentNetEvents.Clear();

            if ((eventMask & (1UL << 36)) != 0) DeserializedDeactivateUmbrellaTalentNetEvents(reader);
            else DeactivateUmbrellaTalentNetEvents.Clear();

            if ((eventMask & (1UL << 37)) != 0) DeserializedLayChickenEggNetEvents(reader);
            else LayChickenEggNetEvents.Clear();

            if ((eventMask & (1UL << 38)) != 0) DeserializedChickenEggHitNetEvents(reader);
            else ChickenEggHitNetEvents.Clear();

            if ((eventMask & (1UL << 39)) != 0) DeserializedActivateYearsOfPainTalentNetEvents(reader);
            else ActivateYearsOfPainTalentNetEvents.Clear();
            
            if ((eventMask & (1UL << 42)) != 0) DeserializedEnvironmentSpikePlayerCollisionEvents(reader);
            else EnvironmentSpikePlayerCollisionNetEvents.Clear();

            if ((eventMask & (1UL << 40)) != 0) DeserializedPlayerPowerUpChangedNetEvents(reader);
            else PlayerPowerUpChangedNetEvents.Clear();

            if ((eventMask & (1UL << 41)) != 0)
            {
                DeserializedActivateSonicSlapNetEvents(reader);
            }
            else
            {
                for (int i = 0; i < ActivateSonicSlapNetEvents.Count; i++)
                    ActivateSonicSlapNetEvents[i].AffectedPlayerIds.Clear();
                ActivateSonicSlapNetEvents.Clear();
            }

            if ((eventMask & (1UL << 43)) != 0) DeserializedPerformGalacticPullNetEvents(reader);
            else PerformGalacticPullNetEvents.Clear();

            if ((eventMask & (1UL << 44)) != 0) DeserializedDeactivateGalacticForceFieldNetEvents(reader);
            else DeactivateGalacticForceFieldNetEvents.Clear();

            if ((eventMask & (1UL << 45)) != 0) DeserializedActivateNukePowerUpNetEvents(reader);
            else ActivateNukePowerUpNetEvents.Clear();

            if ((eventMask & (1UL << 46)) != 0) DeserializedDeactivateShufflePowerUpNetEvents(reader);
            else DeactivateShufflePowerUpNetEvents.Clear();

            if ((eventMask & (1UL << 47)) != 0) DeserializedShuffleSwapPlayerPositionNetEvents(reader);
            else ShuffleSwapPlayerPositionNetEvents.Clear();

            if ((eventMask & (1UL << 48)) != 0) DeserializedActivateShuffleNetEvents(reader);
            else ActivateShuffleNetEvents.Clear();

            if ((eventMask & (1UL << 49)) != 0) DeserializedStartPowerUpGrantingPhaseNetEvents(reader);
            else StartPowerUpGrantingPhaseNetEvents.Clear();

            if ((eventMask & (1UL << 50)) != 0) DeserializedEndPowerUpGrantingPhaseNetEvents(reader);
            else EndPowerUpGrantingPhaseNetEvents.Clear();

            if ((eventMask & (1UL << 51)) != 0) DeserializedActivateWaterGunTalentNetEvents(reader);
            else ActivateWaterGunTalentNetEvents.Clear();

            if ((eventMask & (1UL << 52)) != 0) DeserializedDeactivateWaterGunTalentNetEvents(reader);
            else DeactivateWaterGunTalentNetEvents.Clear();

            if ((eventMask & (1UL << 53)) != 0) DeserializedActivateHeadbuttChargingNetEvents(reader);
            else ActivateHeadbuttChargingNetEvents.Clear();

            if ((eventMask & (1UL << 54)) != 0) DeserializedPerformHeadbuttDashNetEvents(reader);
            else PerformHeadbuttDashNetEvents.Clear();

            if ((eventMask & (1UL << 55)) != 0) DeserializedHeadbuttHitEnemyNetEvents(reader);
            else HeadbuttHitEnemyNetEvents.Clear();

            if ((eventMask & (1UL << 56)) != 0) DeserializedDeactivateHeadbuttTalentNetEvents(reader);
            else DeactivateHeadbuttTalentNetEvents.Clear();

            if ((eventMask & (1UL << 57)) != 0) DeserializedShootFrigidBlockNetEvents(reader);
            else ShootFrigidBlockNetEvents.Clear();

            if ((eventMask & (1UL << 58)) != 0) DeserializedDestroyFrigidBlockNetEvents(reader);
            else DestroyFrigidBlockNetEvents.Clear();

            if ((eventMask & (1UL << 59)) != 0) DeserializedCreateFishingRodProjectileNetEvents(reader);
            else CreateFishingRodProjectileNetEvents.Clear();

            if ((eventMask & (1UL << 60)) != 0) DeserializedFishingRodCaughtEnemyNetEvents(reader);
            else FishingRodCaughtEnemyNetEvents.Clear();

            if ((eventMask & (1UL << 61)) != 0) DeserializedFishingRodTipHitWallNetEvents(reader);
            else FishingRodTipHitWallNetEvents.Clear();

            if ((eventMask & (1UL << 62)) != 0) DeserializedFishingRodThrowNetEvents(reader);
            else FishingRodThrowNetEvents.Clear();

            if ((eventMask & (1UL << 63)) != 0) DeserializedDeactivateFishingRodTalentNetEvents(reader);
            else DeactivateFishingRodTalentNetEvents.Clear();

            if ((eventMask2 & (1UL << 0)) != 0) DeserializedCreateSoulGhostNetEvents(reader);
            else CreateSoulGhostNetEvents.Clear();

            if ((eventMask2 & (1UL << 1)) != 0) DeserializedDeactivateSoulTalentNetEvents(reader);
            else DeactivateSoulTalentNetEvents.Clear();

            if ((eventMask2 & (1UL << 2)) != 0) DeserializedActivateRockTalentNetEvents(reader);
            else ActivateRockTalentNetEvents.Clear();

            if ((eventMask2 & (1UL << 3)) != 0) DeserializedDeactivateRockTalentNetEvents(reader);
            else DeactivateRockTalentNetEvents.Clear();

            if ((eventMask2 & (1UL << 4)) != 0) DeserializedPlayerStartedExposedToLavaNetEvents(reader);
            else PlayerStartedExposedToLavaNetEvents.Clear();

            if ((eventMask2 & (1UL << 5)) != 0) DeserializedPlayerEndedExposedToLavaNetEvents(reader);
            else PlayerEndedExposedToLavaNetEvents.Clear();

            if ((eventMask2 & (1UL << 6)) != 0) DeserializedActivateFrozenTalentNetEvents(reader);
            else ActivateFrozenTalentNetEvents.Clear();

            if ((eventMask2 & (1UL << 7)) != 0) DeserializedDeactivateFrozenTalentNetEvents(reader);
            else DeactivateFrozenTalentNetEvents.Clear();

            if ((eventMask2 & (1UL << 8)) != 0) DeserializedMoleSpawnedNetEvents(reader);
            else MoleSpawnedNetEvents.Clear();

            if ((eventMask2 & (1UL << 9)) != 0) DeserializedMoleHitNetEvents(reader);
            else MoleHitNetEvents.Clear();

            if ((eventMask2 & (1UL << 10)) != 0) DeserializedMoleExpiredNetEvents(reader);
            else MoleExpiredNetEvents.Clear();
        }

        private void SerializedKOProjectHitPlayerNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)KOProjectHitPlayerNetEvents.Count);
            foreach (var netEvent in KOProjectHitPlayerNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedCreateKOProjectileNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateKOProjectileNetEvents.Count);
            foreach (var netEvent in CreateKOProjectileNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDeactivateKOTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateKOTalentNetEvents.Count);
            foreach (var netEvent in DeactivateKOTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }
        
        private void SerializedCreateSwapFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateSwapFieldNetEvents.Count);
            foreach (var netEvent in CreateSwapFieldNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDestroySwapFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DestroySwapFieldNetEvents.Count);
            foreach (var netEvent in DestroySwapFieldNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPreparationPhaseEndedEvents(NetDataWriter writer)
        {
            writer.Put((byte)PreparationPhaseEndedNetEvents.Count);
            foreach (var netEvent in PreparationPhaseEndedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedGainBoltsEvents(NetDataWriter writer)
        {
            writer.Put((byte) GainBoltsNetEvents.Count);
            foreach (var netEvent in GainBoltsNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerToEnvironmentTeleportGateCollisionEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerToEnvironmentTeleportGateCollisionNetEvents.Count);
            foreach (var netEvent in PlayerToEnvironmentTeleportGateCollisionNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedTalentSwitchEvents(NetDataWriter writer)
        {
            writer.Put((byte) TalentSwitchNetEvents.Count);
            foreach (var netEvent in TalentSwitchNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedEnvironmentSpringPlayerCollisionEvents(NetDataWriter writer)
        {
            writer.Put((byte)EnvironmentSpringPlayerCollisionNetEvents.Count);
            foreach (var netEvent in EnvironmentSpringPlayerCollisionNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedEnvironmentSpikePlayerCollisionEvents(NetDataWriter writer)
        {
            writer.Put((byte)EnvironmentSpikePlayerCollisionNetEvents.Count);
            foreach (var netEvent in EnvironmentSpikePlayerCollisionNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedTeamLostEvents(NetDataWriter writer)
        {
            writer.Put((byte) TeamLostNetEvents.Count);
            foreach (var netEvent in TeamLostNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedStageEndEvents(NetDataWriter writer)
        {
            writer.Put((byte) StageEndNetEvents.Count);
            foreach (var netEvent in StageEndNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPowerUpObtainedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PowerUpObtainedNetEvents.Count);
            foreach (var netEvent in PowerUpObtainedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPowerUpSpawnedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PowerUpSpawnedNetEvents.Count);
            foreach (var netEvent in PowerUpSpawnedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedTalentCardHitEvents(NetDataWriter writer)
        {
            writer.Put((byte) TalentCardHitNetEvents.Count);
            foreach (var talentCardHitNetEvent in TalentCardHitNetEvents.AsSpan())
            {
                talentCardHitNetEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerSwapEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerSwapNetEvents.Count);
            foreach (var playerSwapNetEvent in PlayerSwapNetEvents.AsSpan())
            {
                playerSwapNetEvent.Serialize(writer);
            }
        }

        private void SerializedTalentCardObtainedEvents(NetDataWriter writer)
        {
            writer.Put((byte) TalentCardObtainedNetEvents.Count);
            foreach (var talentCardObtained in TalentCardObtainedNetEvents.AsSpan())
            {
                talentCardObtained.Serialize(writer);
            }
        }

        private void DeserializedCreateKOProjectileNetEvents(NetDataReader reader)
        {
            CreateKOProjectileNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateKOProjectileNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedKOProjectHitPlayerNetEvents(NetDataReader reader)
        {
            KOProjectHitPlayerNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref KOProjectHitPlayerNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDeactivateSentryGunTalentNetEvents(NetDataReader reader)
        {
            DeactivateSentryGunTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateSentryGunTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDeactivateKOTalentNetEvents(NetDataReader reader)
        {
            DeactivateKOTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateKOTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }
        private void DeserializedCreateSwapFieldNetEvents(NetDataReader reader)
        {
            CreateSwapFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateSwapFieldNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDestroySwapFieldNetEvents(NetDataReader reader)
        {
            DestroySwapFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DestroySwapFieldNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPreparationPhaseEndedEvents(NetDataReader reader)
        {
            PreparationPhaseEndedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PreparationPhaseEndedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedGainBoltsEvents(NetDataReader reader)
        {
            GainBoltsNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref GainBoltsNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPlayerToEnvironmentTeleportGateCollisionEvents(NetDataReader reader)
        {
            PlayerToEnvironmentTeleportGateCollisionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerToEnvironmentTeleportGateCollisionNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedTalentSwitchEvents(NetDataReader reader)
        {
            TalentSwitchNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref TalentSwitchNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedEnvironmentSpringPlayerCollisionEvents(NetDataReader reader)
        {
            EnvironmentSpringPlayerCollisionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref EnvironmentSpringPlayerCollisionNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedEnvironmentSpikePlayerCollisionEvents(NetDataReader reader)
        {
            EnvironmentSpikePlayerCollisionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref EnvironmentSpikePlayerCollisionNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedTeamLostEvents(NetDataReader reader)
        {
            TeamLostNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref TeamLostNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedStageEndEvents(NetDataReader reader)
        {
            StageEndNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                var netEvent = StageEndNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPowerUpObtainedEvents(NetDataReader reader)
        {
            PowerUpObtainedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PowerUpObtainedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPowerUpSpawnedEvents(NetDataReader reader)
        {
            PowerUpSpawnedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PowerUpSpawnedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedTalentCardHitEvents(NetDataReader reader)
        {
            TalentCardHitNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var talentCardHitEvent = ref TalentCardHitNetEvents.AddAndGet();
                talentCardHitEvent.Deserialize(reader);
            }
        }
        
        private void DeserializedTalentCardObtainedEvents(NetDataReader reader)
        {
            TalentCardObtainedNetEvents.Clear();
            var talentCardObtainedCount = reader.GetByte();
            for (var i = 0; i < talentCardObtainedCount; i++)
            {
                var talentCardObtainedEvent = TalentCardObtainedNetEvents.AddAndGet();
                talentCardObtainedEvent.Deserialize(reader);
            }
        }
        
        private void DeserializedPlayerSwapEvents(NetDataReader reader)
        {
            PlayerSwapNetEvents.Clear();
            var playerSwapEventsCount = reader.GetByte();
            for (var i = 0; i < playerSwapEventsCount; i++)
            {
                ref var playersSwapEvent = ref PlayerSwapNetEvents.AddAndGet();
                playersSwapEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerTakeDamageEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerTakeDamageNetEvents.Count);
            foreach (var playerTakeDamageEvent in PlayerTakeDamageNetEvents.AsSpan())
            {
                playerTakeDamageEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerDiedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerDiedNetEvents.Count);
            foreach (var playerDiedNetEvent in PlayerDiedNetEvents.AsSpan())
            {
                playerDiedNetEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerTakeDamageEvents(NetDataReader reader)
        {
            PlayerTakeDamageNetEvents.Clear();
            var playerTakeDamageEventsCount = reader.GetByte();
            for (var i = 0; i < playerTakeDamageEventsCount; i++)
            {
                ref var playerTakeDamageEvent = ref PlayerTakeDamageNetEvents.AddAndGet();
                playerTakeDamageEvent.Deserialize(reader);
            }
        }

        private void DeserializedPlayerDiedEvents(NetDataReader reader)
        {
            PlayerDiedNetEvents.Clear();
            var playerDiedEventsCount = reader.GetByte();
            for (var i = 0; i < playerDiedEventsCount; i++)
            {
                ref var playerDiedEvent = ref PlayerDiedNetEvents.AddAndGet();
                playerDiedEvent.Deserialize(reader);
            }
        }

        private void SerializedBulletDestroyedEvents(NetDataWriter writer)
        {
            writer.Put((byte) BulletDestroyedNetEvents.Count);
            foreach (var bulletDestroyedEvent in BulletDestroyedNetEvents.AsSpan())
            {
                bulletDestroyedEvent.Serialize(writer);
            }
        }

        private void DeserializedBulletDestroyedEvents(NetDataReader reader)
        {
            BulletDestroyedNetEvents.Clear();
            var bulletDestroyedEventsCount = reader.GetByte();
            for (var i = 0; i < bulletDestroyedEventsCount; i++)
            {
                ref var bulletDestroyedEvent = ref BulletDestroyedNetEvents.AddAndGet();
                bulletDestroyedEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerJoinedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerJoinAcceptNetEvents.Count);
            foreach (var playerJoinAcceptNetEvent in PlayerJoinAcceptNetEvents.AsSpan())
            {
                playerJoinAcceptNetEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerJoinedEvents(NetDataReader reader)
        {
            PlayerJoinAcceptNetEvents.Clear();
            var playerJoinedNetEventsCount = reader.GetByte();
            for (var i = 0; i < playerJoinedNetEventsCount; i++)
            {
                var playerJoinAcceptPacket = PlayerJoinAcceptNetEvents.AddAndGet();
                playerJoinAcceptPacket.Deserialize(reader);
            }
        }

        private void SerializedBulletSpawnedEvents(NetDataWriter writer)
        {
            var bulletSpawnedAmount = BulletSpawnNetEvents.Count;
            writer.Put((byte) bulletSpawnedAmount);
            foreach (var bulletSpawnEvent in BulletSpawnNetEvents.AsSpan())
            {
                bulletSpawnEvent.Serialize(writer);
            }
        }

        private void DeserializedBulletSpawnedEvents(NetDataReader reader)
        {
            BulletSpawnNetEvents.Clear();
            var bulletSpawnNetEventsCount = reader.GetByte();
            for (var i = 0; i < bulletSpawnNetEventsCount; i++)
            {
                ref var bulletSpawnEvent = ref BulletSpawnNetEvents.AddAndGet();
                bulletSpawnEvent.Deserialize(reader);
            }
        }

        private void SerializedPerformDashPulseNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PerformDashPulseNetEvents.Count);
            foreach (var netEvent in PerformDashPulseNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedPerformDashPulseNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            PerformDashPulseNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref PerformDashPulseNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateSentryGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateSentryGunTalentNetEvents.Count);
            foreach (var netEvent in ActivateSentryGunTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDeactivateSentryGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateSentryGunTalentNetEvents.Count);
            foreach (var netEvent in DeactivateSentryGunTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }
        
        private void DeserializedActivateSentryGunTalentNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            ActivateSentryGunTalentNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateSentryGunTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerSpinnedStartedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerSpinnedStartedNetEvents.Count);
            foreach (var evt in PlayerSpinnedStartedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedPlayerSpinnedStartedEvents(NetDataReader reader)
        {
            PlayerSpinnedStartedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PlayerSpinnedStartedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedPlayerSpinnedEndedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerSpinnedEndedNetEvents.Count);
            foreach (var evt in PlayerSpinnedEndedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedPlayerSpinnedEndedEvents(NetDataReader reader)
        {
            PlayerSpinnedEndedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PlayerSpinnedEndedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedUpdatePlayerTalentStocksNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)UpdatePlayerTalentStocksNetEvents.Count);
            foreach (var netEvent in UpdatePlayerTalentStocksNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedPlayerMaxShootCooldownChangedNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerMaxShootCooldownChangedNetEvents.Count);
            foreach (var netEvent in PlayerMaxShootCooldownChangedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedUpdatePlayerTalentStocksNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            UpdatePlayerTalentStocksNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref UpdatePlayerTalentStocksNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedPlayerMaxShootCooldownChangedNetEvents(NetDataReader reader)
        {
            var count = reader.GetByte();
            PlayerMaxShootCooldownChangedNetEvents.Clear();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerMaxShootCooldownChangedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedCreateGrapplingHookProjectileNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateGrapplingHookProjectileNetEvents.Count);
            foreach (var netEvent in CreateGrapplingHookProjectileNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedGrapplingHookHitWallNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)GrapplingHookHitWallNetEvents.Count);
            foreach (var netEvent in GrapplingHookHitWallNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void SerializedDeactivateGrapplingHookTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateGrapplingHookTalentNetEvents.Count);
            foreach (var netEvent in DeactivateGrapplingHookTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedCreateGrapplingHookProjectileNetEvents(NetDataReader reader)
        {
            CreateGrapplingHookProjectileNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateGrapplingHookProjectileNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedGrapplingHookHitWallNetEvents(NetDataReader reader)
        {
            GrapplingHookHitWallNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref GrapplingHookHitWallNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void DeserializedDeactivateGrapplingHookTalentNetEvents(NetDataReader reader)
        {
            DeactivateGrapplingHookTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateGrapplingHookTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateUmbrellaTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateUmbrellaTalentNetEvents.Count);
            foreach (var netEvent in ActivateUmbrellaTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedActivateUmbrellaTalentNetEvents(NetDataReader reader)
        {
            ActivateUmbrellaTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateUmbrellaTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateUmbrellaTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateUmbrellaTalentNetEvents.Count);
            foreach (var netEvent in DeactivateUmbrellaTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedDeactivateUmbrellaTalentNetEvents(NetDataReader reader)
        {
            DeactivateUmbrellaTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateUmbrellaTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedCreateMagneticPullFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateMagneticPullFieldNetEvents.Count);
            foreach (var netEvent in CreateMagneticPullFieldNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedCreateMagneticPullFieldNetEvents(NetDataReader reader)
        {
            CreateMagneticPullFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateMagneticPullFieldNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }
        
        private void SerializedLayChickenEggNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)LayChickenEggNetEvents.Count);

            foreach (var netEvent in LayChickenEggNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedLayChickenEggNetEvents(NetDataReader reader)
        {
            LayChickenEggNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref LayChickenEggNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedChickenEggHitNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ChickenEggHitNetEvents.Count);

            foreach (var netEvent in ChickenEggHitNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedChickenEggHitNetEvents(NetDataReader reader)
        {
            ChickenEggHitNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ChickenEggHitNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateYearsOfPainTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateYearsOfPainTalentNetEvents.Count);
            foreach (var netEvent in ActivateYearsOfPainTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedActivateYearsOfPainTalentNetEvents(NetDataReader reader)
        {
            ActivateYearsOfPainTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateYearsOfPainTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerLockOnTargetsChangedNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerLockOnTargetsChangedNetEvents.Count);
            foreach (var netEvent in PlayerLockOnTargetsChangedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerLockOnTargetsChangedNetEvents(NetDataReader reader)
        {
            for (int i = 0; i < PlayerLockOnTargetsChangedNetEvents.Count; i++)
            {
                PlayerLockOnTargetsChangedNetEvents[i].LockedOnTargetObjects.Clear();
            }
            
            PlayerLockOnTargetsChangedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                var netEvent = PlayerLockOnTargetsChangedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerLockedOnTargetHitNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerLockedOnTargetHitNetEvents.Count);
            foreach (var netEvent in PlayerLockedOnTargetHitNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerLockedOnTargetHitNetEvents(NetDataReader reader)
        {
            PlayerLockedOnTargetHitNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerLockedOnTargetHitNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerPowerUpChangedNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerPowerUpChangedNetEvents.Count);
            foreach (var netEvent in PlayerPowerUpChangedNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerPowerUpChangedNetEvents(NetDataReader reader)
        {
            PlayerPowerUpChangedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerPowerUpChangedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateSonicSlapNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateSonicSlapNetEvents.Count);
            foreach (var netEvent in ActivateSonicSlapNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedActivateSonicSlapNetEvents(NetDataReader reader)
        {
            for (int i = 0; i < ActivateSonicSlapNetEvents.Count; i++)
            {
                ActivateSonicSlapNetEvents[i].AffectedPlayerIds.Clear();
            }

            ActivateSonicSlapNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                var netEvent = ActivateSonicSlapNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPerformGalacticPullNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PerformGalacticPullNetEvents.Count);
            foreach (var netEvent in PerformGalacticPullNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedPerformGalacticPullNetEvents(NetDataReader reader)
        {
            PerformGalacticPullNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref PerformGalacticPullNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateGalacticForceFieldNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateGalacticForceFieldNetEvents.Count);
            foreach (var netEvent in DeactivateGalacticForceFieldNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDeactivateGalacticForceFieldNetEvents(NetDataReader reader)
        {
            DeactivateGalacticForceFieldNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateGalacticForceFieldNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateNukePowerUpNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateNukePowerUpNetEvents.Count);
            foreach (var netEvent in ActivateNukePowerUpNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedActivateNukePowerUpNetEvents(NetDataReader reader)
        {
            ActivateNukePowerUpNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateNukePowerUpNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateShufflePowerUpNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateShufflePowerUpNetEvents.Count);
            foreach (var netEvent in DeactivateShufflePowerUpNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDeactivateShufflePowerUpNetEvents(NetDataReader reader)
        {
            DeactivateShufflePowerUpNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateShufflePowerUpNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedShuffleSwapPlayerPositionNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ShuffleSwapPlayerPositionNetEvents.Count);
            foreach (var netEvent in ShuffleSwapPlayerPositionNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedShuffleSwapPlayerPositionNetEvents(NetDataReader reader)
        {
            ShuffleSwapPlayerPositionNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref ShuffleSwapPlayerPositionNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateShuffleNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateShuffleNetEvents.Count);
            foreach (var netEvent in ActivateShuffleNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedActivateShuffleNetEvents(NetDataReader reader)
        {
            ActivateShuffleNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateShuffleNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedStartPowerUpGrantingPhaseNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)StartPowerUpGrantingPhaseNetEvents.Count);
            foreach (var netEvent in StartPowerUpGrantingPhaseNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedStartPowerUpGrantingPhaseNetEvents(NetDataReader reader)
        {
            StartPowerUpGrantingPhaseNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref StartPowerUpGrantingPhaseNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedEndPowerUpGrantingPhaseNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)EndPowerUpGrantingPhaseNetEvents.Count);
            foreach (var netEvent in EndPowerUpGrantingPhaseNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedEndPowerUpGrantingPhaseNetEvents(NetDataReader reader)
        {
            EndPowerUpGrantingPhaseNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var netEvent = ref EndPowerUpGrantingPhaseNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateWaterGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateWaterGunTalentNetEvents.Count);
            foreach (var netEvent in ActivateWaterGunTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedActivateWaterGunTalentNetEvents(NetDataReader reader)
        {
            ActivateWaterGunTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateWaterGunTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateWaterGunTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateWaterGunTalentNetEvents.Count);
            foreach (var netEvent in DeactivateWaterGunTalentNetEvents.AsSpan())
            {
                netEvent.Serialize(writer);
            }
        }

        private void DeserializedDeactivateWaterGunTalentNetEvents(NetDataReader reader)
        {
            DeactivateWaterGunTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateWaterGunTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateHeadbuttChargingNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateHeadbuttChargingNetEvents.Count);
            foreach (var netEvent in ActivateHeadbuttChargingNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedActivateHeadbuttChargingNetEvents(NetDataReader reader)
        {
            ActivateHeadbuttChargingNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateHeadbuttChargingNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPerformHeadbuttDashNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PerformHeadbuttDashNetEvents.Count);
            foreach (var netEvent in PerformHeadbuttDashNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedPerformHeadbuttDashNetEvents(NetDataReader reader)
        {
            PerformHeadbuttDashNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref PerformHeadbuttDashNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedHeadbuttHitEnemyNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)HeadbuttHitEnemyNetEvents.Count);
            foreach (var netEvent in HeadbuttHitEnemyNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedHeadbuttHitEnemyNetEvents(NetDataReader reader)
        {
            HeadbuttHitEnemyNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref HeadbuttHitEnemyNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateHeadbuttTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateHeadbuttTalentNetEvents.Count);
            foreach (var netEvent in DeactivateHeadbuttTalentNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDeactivateHeadbuttTalentNetEvents(NetDataReader reader)
        {
            DeactivateHeadbuttTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateHeadbuttTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedShootFrigidBlockNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ShootFrigidBlockNetEvents.Count);
            foreach (var netEvent in ShootFrigidBlockNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedShootFrigidBlockNetEvents(NetDataReader reader)
        {
            ShootFrigidBlockNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ShootFrigidBlockNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDestroyFrigidBlockNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DestroyFrigidBlockNetEvents.Count);
            foreach (var netEvent in DestroyFrigidBlockNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDestroyFrigidBlockNetEvents(NetDataReader reader)
        {
            DestroyFrigidBlockNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DestroyFrigidBlockNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedCreateFishingRodProjectileNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateFishingRodProjectileNetEvents.Count);
            foreach (var netEvent in CreateFishingRodProjectileNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedCreateFishingRodProjectileNetEvents(NetDataReader reader)
        {
            CreateFishingRodProjectileNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateFishingRodProjectileNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedFishingRodCaughtEnemyNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)FishingRodCaughtEnemyNetEvents.Count);
            foreach (var netEvent in FishingRodCaughtEnemyNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedFishingRodCaughtEnemyNetEvents(NetDataReader reader)
        {
            FishingRodCaughtEnemyNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref FishingRodCaughtEnemyNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedFishingRodTipHitWallNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)FishingRodTipHitWallNetEvents.Count);
            foreach (var netEvent in FishingRodTipHitWallNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedFishingRodTipHitWallNetEvents(NetDataReader reader)
        {
            FishingRodTipHitWallNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref FishingRodTipHitWallNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedFishingRodThrowNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)FishingRodThrowNetEvents.Count);
            foreach (var netEvent in FishingRodThrowNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedFishingRodThrowNetEvents(NetDataReader reader)
        {
            FishingRodThrowNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref FishingRodThrowNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateFishingRodTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateFishingRodTalentNetEvents.Count);
            foreach (var netEvent in DeactivateFishingRodTalentNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDeactivateFishingRodTalentNetEvents(NetDataReader reader)
        {
            DeactivateFishingRodTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateFishingRodTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedCreateSoulGhostNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)CreateSoulGhostNetEvents.Count);
            foreach (var netEvent in CreateSoulGhostNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedCreateSoulGhostNetEvents(NetDataReader reader)
        {
            CreateSoulGhostNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref CreateSoulGhostNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateSoulTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateSoulTalentNetEvents.Count);
            foreach (var netEvent in DeactivateSoulTalentNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDeactivateSoulTalentNetEvents(NetDataReader reader)
        {
            DeactivateSoulTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateSoulTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateRockTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateRockTalentNetEvents.Count);
            foreach (var netEvent in ActivateRockTalentNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedActivateRockTalentNetEvents(NetDataReader reader)
        {
            ActivateRockTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateRockTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateRockTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateRockTalentNetEvents.Count);
            foreach (var netEvent in DeactivateRockTalentNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDeactivateRockTalentNetEvents(NetDataReader reader)
        {
            DeactivateRockTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateRockTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedActivateFrozenTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)ActivateFrozenTalentNetEvents.Count);
            foreach (var netEvent in ActivateFrozenTalentNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedActivateFrozenTalentNetEvents(NetDataReader reader)
        {
            ActivateFrozenTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref ActivateFrozenTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedDeactivateFrozenTalentNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)DeactivateFrozenTalentNetEvents.Count);
            foreach (var netEvent in DeactivateFrozenTalentNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedDeactivateFrozenTalentNetEvents(NetDataReader reader)
        {
            DeactivateFrozenTalentNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref DeactivateFrozenTalentNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerStartedExposedToLavaNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerStartedExposedToLavaNetEvents.Count);
            foreach (var netEvent in PlayerStartedExposedToLavaNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedPlayerStartedExposedToLavaNetEvents(NetDataReader reader)
        {
            PlayerStartedExposedToLavaNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerStartedExposedToLavaNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerEndedExposedToLavaNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerEndedExposedToLavaNetEvents.Count);
            foreach (var netEvent in PlayerEndedExposedToLavaNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedPlayerEndedExposedToLavaNetEvents(NetDataReader reader)
        {
            PlayerEndedExposedToLavaNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref PlayerEndedExposedToLavaNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedMoleSpawnedNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)MoleSpawnedNetEvents.Count);
            foreach (var netEvent in MoleSpawnedNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedMoleSpawnedNetEvents(NetDataReader reader)
        {
            MoleSpawnedNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref MoleSpawnedNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedMoleHitNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)MoleHitNetEvents.Count);
            foreach (var netEvent in MoleHitNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedMoleHitNetEvents(NetDataReader reader)
        {
            MoleHitNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref MoleHitNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

        private void SerializedMoleExpiredNetEvents(NetDataWriter writer)
        {
            writer.Put((byte)MoleExpiredNetEvents.Count);
            foreach (var netEvent in MoleExpiredNetEvents.AsSpan())
                netEvent.Serialize(writer);
        }

        private void DeserializedMoleExpiredNetEvents(NetDataReader reader)
        {
            MoleExpiredNetEvents.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var netEvent = ref MoleExpiredNetEvents.AddAndGet();
                netEvent.Deserialize(reader);
            }
        }

    }
}
