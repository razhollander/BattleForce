using System.Collections;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.LocalEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents
{
    public interface ICachedPresentationEventsService
    {
        List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; }
        List<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents { get; }
        List<PlayerDiedNetEventS2C> PlayerDiedNetEvents { get;}
        List<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents { get; }
        List<PlayersSwapNetEventS2C> PlayerSwapNetEvents { get; }
        List<TalentCardObtainedNetEventS2C> TalentCardObtainedNetEvents { get; }
        List<TalentCardHitNetEventS2C> TalentCardHitNetEvents { get; }
        List<PlayerSpinnedStartedNetEventS2C> PlayerSpinnedStartedNetEvents { get; }
        List<PlayerSpinnedEndedNetEventS2C> PlayerSpinnedEndedNetEvents { get; }
        List<PlayerStartedExposedToLavaNetEventS2C> PlayerStartedExposedToLavaNetEvents { get; }
        List<PlayerEndedExposedToLavaNetEventS2C> PlayerEndedExposedToLavaNetEvents { get; }
        List<PowerUpBallSpawnedNetEventS2C> PowerUpBallSpawnedNetEvents { get; }
        List<PowerUpBallObtainedNetEventS2C> PowerUpBallObtainedNetEvents { get; }
        List<MoleSpawnedNetEventS2C> MoleSpawnedNetEvents { get; }
        List<MoleHitNetEventS2C> MoleHitNetEvents { get; }
        List<ScoreGatePassedNetEventS2C> ScoreGatePassedNetEvents { get; }
        List<MoleExpiredNetEventS2C> MoleExpiredNetEvents { get; }
        List<GoldenMoleDamagedNetEventS2C> GoldenMoleDamagedNetEvents { get; }
        List<PlayerSwitchTeamNetEventS2C> PlayerSwitchTeamNetEvents { get; }
        List<StageEndNetEventS2C> StageEndNetEvents { get; }
        List<TeamLostNetEventS2C> TeamLostNetEvents { get; }
        List<TalentSwitchNetEventS2C> TalentSwitchNetEvents { get; }
        List<GainBoltsNetEventS2C> GainBoltsNetEvents { get; }
        List<EnvironmentSpringPlayerCollisionNetEventS2C> EnvironmentSpringPlayerCollisionNetEvents { get; }
        List<EnvironmentSpikePlayerCollisionNetEventS2C> EnvironmentSpikePlayerCollisionNetEvents { get; }
        List<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> PlayerToEnvironmentTeleportGateCollisionNetEvents { get; }
        List<PreparationPhaseEndedNetEventS2C> PreparationPhaseEndedNetEvents { get; }
        List<CreateSwapFieldNetEventS2C> CreateSwapFieldNetEvents { get; }
        List<DeactivateSwapTalentNetEventS2C> DeactivateSwapTalentNetEvents { get; }
        List<KOProjectHitPlayerNetEventS2C> KOProjectHitPlayerNetEvents { get; }
        List<CreateKOProjectileNetEventS2C> CreateKOProjectileNetEvents { get; }
        List<DeactivateKOTalentNetEventS2C> DeactivateKOTalentNetEvents { get; }
        List<CreateGrapplingHookProjectileNetEventS2C> CreateGrapplingHookProjectileNetEvents { get; }
        List<GrapplingHookHitWallNetEventS2C> GrapplingHookHitWallNetEvents { get; }
        List<DeactivateGrapplingHookTalentNetEventS2C> DeactivateGrapplingHookTalentNetEvents { get; }
        List<CreateFishingRodProjectileNetEventS2C> CreateFishingRodProjectileNetEvents { get; }
        List<FishingRodCaughtEnemyNetEventS2C> FishingRodCaughtEnemyNetEvents { get; }
        List<FishingRodTipHitWallNetEventS2C> FishingRodTipHitWallNetEvents { get; }
        List<FishingRodThrowNetEventS2C> FishingRodThrowNetEvents { get; }
        List<DeactivateFishingRodTalentNetEventS2C> DeactivateFishingRodTalentNetEvents { get; }
        List<CreateSoulGhostNetEventS2C> CreateSoulGhostNetEvents { get; }
        List<DeactivateSoulTalentNetEventS2C> DeactivateSoulTalentNetEvents { get; }
        List<ActivateRockTalentNetEventS2C> ActivateRockTalentNetEvents { get; }
        List<DeactivateRockTalentNetEventS2C> DeactivateRockTalentNetEvents { get; }
        List<ActivateFrozenTalentNetEventS2C> ActivateFrozenTalentNetEvents { get; }
        List<DeactivateFrozenTalentNetEventS2C> DeactivateFrozenTalentNetEvents { get; }
        List<ShootFrigidBlockNetEventS2C> ShootFrigidBlockNetEvents { get; }
        List<DestroyFrigidBlockNetEventS2C> DestroyFrigidBlockNetEvents { get; }
        List<ActivateSentryGunTalentNetEventS2C> ActivateSentryGunTalentNetEvents { get; }
        List<DeactivateSentryGunTalentNetEventS2C> DeactivateSentryGunTalentNetEvents { get; }
        List<PerformDashPulseNetEventS2C> PerformDashPulseNetEvents { get; }
        List<UpdatePlayerTalentStocksNetEventS2C> UpdatePlayerTalentStocksNetEvents { get; }
        List<PlayerSelectedTalentFinishedCooldownLocalEvent> PlayerSelectedTalentFinishedCooldownLocalEvents { get; }
        List<ActivateUmbrellaTalentNetEventS2C> ActivateUmbrellaTalentNetEvents { get; }
        List<DeactivateUmbrellaTalentNetEventS2C> DeactivateUmbrellaTalentNetEvents { get; }
        List<ActivateWaterGunTalentNetEventS2C> ActivateWaterGunTalentNetEvents { get; }
        List<DeactivateWaterGunTalentNetEventS2C> DeactivateWaterGunTalentNetEvents { get; }
        List<ActivateHeadbuttChargingNetEventS2C> ActivateHeadbuttChargingNetEvents { get; }
        List<PerformHeadbuttDashNetEventS2C> PerformHeadbuttDashNetEvents { get; }
        List<HeadbuttHitEnemyNetEventS2C> HeadbuttHitEnemyNetEvents { get; }
        List<DeactivateHeadbuttTalentNetEventS2C> DeactivateHeadbuttTalentNetEvents { get; }
        List<CreateMagneticPullFieldNetEventS2C> CreateMagenticPullFieldNetEvents { get; }
        List<LayChickenEggNetEventS2C> LayChickenEggNetEvents { get; }
        List<ChickenEggHitNetEventS2C> ChickenEggHitNetEvents { get; }
        List<ActivateYearsOfPainTalentNetEventS2C> ActivateYearsOfPainTalentNetEvents { get; }
        List<PlayerLockOnTargetsChangedNetEventS2C> PlayerLockOnTargetsChangedNetEvents { get; }
        List<PlayerLockedOnTargetHitNetEventS2C> PlayerLockedOnTargetHitNetEvents { get; }
        List<PlayerPowerUpChangedNetEventS2C> PlayerPowerUpChangedNetEvents { get; }
        List<ActivateSonicSlapNetEventS2C> ActivateSonicSlapNetEvents { get; }
        List<PerformGalacticPullNetEventS2C> PerformGalacticPullNetEvents { get; }
        List<DeactivateGalacticForceFieldNetEventS2C> DeactivateGalacticForceFieldNetEvents { get; }
        List<ActivateNukePowerUpNetEventS2C> ActivateNukePowerUpNetEvents { get; }
        List<DeactivateShufflePowerUpNetEventS2C> DeactivateShufflePowerUpNetEvents { get; }
        List<ShuffleSwapPlayerPositionNetEventS2C> ShuffleSwapPlayerPositionNetEvents { get; }
        List<ActivateShuffleNetEventS2C> ActivateShuffleNetEvents { get; }
        List<StartPowerUpGrantingPhaseNetEventS2C> StartPowerUpGrantingPhaseNetEvents { get; }
        List<EndPowerUpGrantingPhaseNetEventS2C> EndPowerUpGrantingPhaseNetEvents { get; }
    }
}