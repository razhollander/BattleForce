using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents
{
    public class CachedPresentationEventsService : ICachedPresentationEventsService
    {
        public List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; set; } = new ();
        public List<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents { get; set; } = new();
        public List<PlayerDiedNetEventS2C> PlayerDiedNetEvents { get; set; } = new();
        public List<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents { get; set; } = new();
        public List<PlayersSwapNetEventS2C> PlayerSwapNetEvents { get; set; } = new();
        public List<TalentCardObtainedNetEventS2C> TalentCardObtainedNetEvents { get; set; } = new();
        public List<TalentCardHitNetEventS2C> TalentCardHitNetEvents { get; set; } = new();
        public List<PowerUpBallSpawnedNetEventS2C> PowerUpBallSpawnedNetEvents { get; set; } = new();
        public List<PowerUpBallObtainedNetEventS2C> PowerUpBallObtainedNetEvents { get; set; } = new();
        public List<PlayerSwitchTeamNetEventS2C> PlayerSwitchTeamNetEvents { get; set; } = new();
        public List<StageEndNetEventS2C> StageEndNetEvents { get; set; } = new();
        public List<TeamLostNetEventS2C> TeamLostNetEvents { get; set; } = new();
        public List<TalentSwitchNetEventS2C> TalentSwitchNetEvents { get; set; } = new();
        public List<GainBoltsNetEventS2C> GainBoltsNetEvents { get; set; } = new();
        public List<EnvironmentSpringPlayerCollisionNetEventS2C> EnvironmentSpringPlayerCollisionNetEvents { get; set; } = new();
        public List<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> PlayerToEnvironmentTeleportGateCollisionNetEvents { get; set; } = new();
    }
}