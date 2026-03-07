using System.Collections;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents
{
    public interface ICachedPresentationEventsService
    {
        List<BulletSpawnNetEventS2C> BulletSpawnNetEvents { get; set; }
        List<PlayerTakeDamageNetEventS2C> PlayerTakeDamageNetEvents { get; set; }
        List<PlayerDiedNetEventS2C> PlayerDiedNetEvents { get; set; }
        List<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents { get; set; }
        List<PlayersSwapNetEventS2C> PlayerSwapNetEvents { get; set; }
        List<TalentCardObtainedNetEventS2C> TalentCardObtainedNetEvents { get; set; }
        List<TalentCardHitNetEventS2C> TalentCardHitNetEvents { get; set; }
        List<PowerUpBallSpawnedNetEventS2C> PowerUpBallSpawnedNetEvents { get; set; }
        List<PowerUpBallObtainedNetEventS2C> PowerUpBallObtainedNetEvents { get; set; }
        List<PlayerSwitchTeamNetEventS2C> PlayerSwitchTeamNetEvents { get; set; }
        List<StageEndNetEventS2C> StageEndNetEvents { get; set; }
        List<TeamLostNetEventS2C> TeamLostNetEvents { get; set; }
        List<TalentSwitchNetEventS2C> TalentSwitchNetEvents { get; set; }
        List<GainBoltsNetEventS2C> GainBoltsNetEvents { get; set; }
        List<EnvironmentSpringPlayerCollisionNetEventS2C> EnvironmentSpringPlayerCollisionNetEvents { get; set; }
        List<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> PlayerToEnvironmentTeleportGateCollisionNetEvents { get; set; }
        List<PreparationPhaseEndedNetEventS2C> PreparationPhaseEndedNetEvents { get; set; }
    }
}