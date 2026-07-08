using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.LocalEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents
{
    public class CachedPresentationEventsService : ICachedPresentationEventsService
    {
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; } = new ();
        public List<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents { get; } = new();
        public List<PlayerDiedNetEventS2C> PlayerDiedNetEvents { get; } = new();
        public List<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents { get; } = new();
        public List<PlayersSwapNetEventS2C> PlayerSwapNetEvents { get; } = new();
        public List<TalentCardObtainedNetEventS2C> TalentCardObtainedNetEvents { get; } = new();
        public List<TalentCardHitNetEventS2C> TalentCardHitNetEvents { get; } = new();
        public List<PlayerSpinnedStartedNetEventS2C> PlayerSpinnedStartedNetEvents { get; } = new();
        public List<PlayerSpinnedEndedNetEventS2C> PlayerSpinnedEndedNetEvents { get; } = new();
        public List<PowerUpBallSpawnedNetEventS2C> PowerUpBallSpawnedNetEvents { get; } = new();
        public List<PowerUpBallObtainedNetEventS2C> PowerUpBallObtainedNetEvents { get; } = new();
        public List<PlayerSwitchTeamNetEventS2C> PlayerSwitchTeamNetEvents { get; } = new();
        public List<StageEndNetEventS2C> StageEndNetEvents { get; } = new();
        public List<TeamLostNetEventS2C> TeamLostNetEvents { get; } = new();
        public List<TalentSwitchNetEventS2C> TalentSwitchNetEvents { get; } = new();
        public List<GainBoltsNetEventS2C> GainBoltsNetEvents { get; } = new();
        public List<EnvironmentSpringPlayerCollisionNetEventS2C> EnvironmentSpringPlayerCollisionNetEvents { get; } = new();
        public List<Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents.EnvironmentSpikePlayerCollisionNetEventS2C> EnvironmentSpikePlayerCollisionNetEvents { get; } = new();
        public List<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> PlayerToEnvironmentTeleportGateCollisionNetEvents { get; } = new();
        public List<PreparationPhaseEndedNetEventS2C> PreparationPhaseEndedNetEvents { get; } = new();
        public List<CreateSwapFieldNetEventS2C> CreateSwapFieldNetEvents { get; } = new();
        public List<DeactivateSwapTalentNetEventS2C> DeactivateSwapTalentNetEvents { get; } = new();
        public List<KOProjectHitPlayerNetEventS2C> KOProjectHitPlayerNetEvents { get; } = new();
        public List<CreateKOProjectileNetEventS2C> CreateKOProjectileNetEvents { get; } = new();
        public List<DeactivateKOTalentNetEventS2C> DeactivateKOTalentNetEvents { get; } = new();
        public List<CreateGrapplingHookProjectileNetEventS2C> CreateGrapplingHookProjectileNetEvents { get; } = new();
        public List<GrapplingHookHitWallNetEventS2C> GrapplingHookHitWallNetEvents { get; } = new();
        public List<DeactivateGrapplingHookTalentNetEventS2C> DeactivateGrapplingHookTalentNetEvents { get; } = new();
        public List<CreateFishingRodProjectileNetEventS2C> CreateFishingRodProjectileNetEvents { get; } = new();
        public List<FishingRodCaughtEnemyNetEventS2C> FishingRodCaughtEnemyNetEvents { get; } = new();
        public List<FishingRodTipHitWallNetEventS2C> FishingRodTipHitWallNetEvents { get; } = new();
        public List<FishingRodThrowNetEventS2C> FishingRodThrowNetEvents { get; } = new();
        public List<DeactivateFishingRodTalentNetEventS2C> DeactivateFishingRodTalentNetEvents { get; } = new();
        public List<CreateSoulGhostNetEventS2C> CreateSoulGhostNetEvents { get; } = new();
        public List<DeactivateSoulTalentNetEventS2C> DeactivateSoulTalentNetEvents { get; } = new();
        public List<ShootFrigidBlockNetEventS2C> ShootFrigidBlockNetEvents { get; } = new();
        public List<DestroyFrigidBlockNetEventS2C> DestroyFrigidBlockNetEvents { get; } = new();
        public List<ActivateSentryGunTalentNetEventS2C> ActivateSentryGunTalentNetEvents { get; } = new();
        public List<DeactivateSentryGunTalentNetEventS2C> DeactivateSentryGunTalentNetEvents { get; } = new();
        public List<PerformDashPulseNetEventS2C> PerformDashPulseNetEvents { get; } = new();
        public List<UpdatePlayerTalentStocksNetEventS2C> UpdatePlayerTalentStocksNetEvents { get; } = new();
        public List<PlayerSelectedTalentFinishedCooldownLocalEvent> PlayerSelectedTalentFinishedCooldownLocalEvents { get; } = new();
        public List<ActivateUmbrellaTalentNetEventS2C> ActivateUmbrellaTalentNetEvents { get; } = new();
        public List<DeactivateUmbrellaTalentNetEventS2C> DeactivateUmbrellaTalentNetEvents { get; } = new();
        public List<ActivateWaterGunTalentNetEventS2C> ActivateWaterGunTalentNetEvents { get; } = new();
        public List<DeactivateWaterGunTalentNetEventS2C> DeactivateWaterGunTalentNetEvents { get; } = new();
        public List<ActivateHeadbuttChargingNetEventS2C> ActivateHeadbuttChargingNetEvents { get; } = new();
        public List<PerformHeadbuttDashNetEventS2C> PerformHeadbuttDashNetEvents { get; } = new();
        public List<HeadbuttHitEnemyNetEventS2C> HeadbuttHitEnemyNetEvents { get; } = new();
        public List<DeactivateHeadbuttTalentNetEventS2C> DeactivateHeadbuttTalentNetEvents { get; } = new();
        public List<CreateMagneticPullFieldNetEventS2C> CreateMagenticPullFieldNetEvents { get; } = new();
        public List<LayChickenEggNetEventS2C> LayChickenEggNetEvents { get; } = new();
        public List<ChickenEggHitNetEventS2C> ChickenEggHitNetEvents { get; } = new();
        public List<ActivateYearsOfPainTalentNetEventS2C> ActivateYearsOfPainTalentNetEvents { get; } = new();
        public List<PlayerLockOnTargetsChangedNetEventS2C> PlayerLockOnTargetsChangedNetEvents { get; } = new();
        public List<PlayerLockedOnTargetHitNetEventS2C> PlayerLockedOnTargetHitNetEvents { get; } = new();
        public List<PlayerPowerUpChangedNetEventS2C> PlayerPowerUpChangedNetEvents { get; } = new();
        public List<ActivateSonicSlapNetEventS2C> ActivateSonicSlapNetEvents { get; } = new();
        public List<PerformGalacticPullNetEventS2C> PerformGalacticPullNetEvents { get; } = new();
        public List<DeactivateGalacticForceFieldNetEventS2C> DeactivateGalacticForceFieldNetEvents { get; } = new();
        public List<ActivateNukePowerUpNetEventS2C> ActivateNukePowerUpNetEvents { get; } = new();
        public List<DeactivateShufflePowerUpNetEventS2C> DeactivateShufflePowerUpNetEvents { get; } = new();
        public List<ShuffleSwapPlayerPositionNetEventS2C> ShuffleSwapPlayerPositionNetEvents { get; } = new();
        public List<ActivateShuffleNetEventS2C> ActivateShuffleNetEvents { get; } = new();
        public List<StartPowerUpGrantingPhaseNetEventS2C> StartPowerUpGrantingPhaseNetEvents { get; } = new();
        public List<EndPowerUpGrantingPhaseNetEventS2C> EndPowerUpGrantingPhaseNetEvents { get; } = new();
    }
}