using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public interface IKOProjectilesControllers
    {
        void UpdateKOProjectilesTransform();
        void HandleCreateEvents(List<CreateKOProjectileNetEventS2C> events);
        void HandleDeactivateEvents(List<DeactivateKOTalentNetEventS2C> events);
    }
}
