using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
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
        public List<PowerUpBallSpawnedNetEventS2C> PowerUpBallSpawnedNetEvents { get; } = new();
        public List<PowerUpBallObtainedNetEventS2C> PowerUpBallObtainedNetEvents { get; } = new();
        public List<PlayerSwitchTeamNetEventS2C> PlayerSwitchTeamNetEvents { get; } = new();
        public List<StageEndNetEventS2C> StageEndNetEvents { get; } = new();
        public List<TeamLostNetEventS2C> TeamLostNetEvents { get; } = new();
        public List<TalentSwitchNetEventS2C> TalentSwitchNetEvents { get; } = new();
        public List<GainBoltsNetEventS2C> GainBoltsNetEvents { get; } = new();
        public List<EnvironmentSpringPlayerCollisionNetEventS2C> EnvironmentSpringPlayerCollisionNetEvents { get; } = new();
        public List<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> PlayerToEnvironmentTeleportGateCollisionNetEvents { get; } = new();
        public List<PreparationPhaseEndedNetEventS2C> PreparationPhaseEndedNetEvents { get; } = new();
        public List<CreateSwapFieldNetEventS2C> CreateSwapFieldNetEvents { get; } = new();
        public List<DeactivateSwapTalentNetEventS2C> DeactivateSwapTalentNetEvents { get; } = new();
        public List<KOProjectHitPlayerNetEventS2C> KOProjectHitPlayerNetEvents { get; } = new();
        public List<CreateKOProjectileNetEventS2C> CreateKOProjectileNetEvents { get; } = new();
        public List<DeactivateKOTalentNetEventS2C> DeactivateKOTalentNetEvents { get; } = new();
        public List<ActivateSentryGunTalentNetEventS2C> ActivateSentryGunTalentNetEvents { get; } = new();
        public List<DeactivateSentryGunTalentNetEventS2C> DeactivateSentryGunTalentNetEvents { get; } = new();
        public List<PerformDashPulseNetEventS2C> PerformDashPulseNetEvents { get; } = new();
        // public List<DeactivateDashPulseTalentNetEventS2C> DeactivateDashPulseTalentNetEvents { get; } = new();
        public List<UpdatePlayerTalentStocksNetEventS2C> UpdatePlayerTalentStocksNetEvents { get; } = new();
    }
}