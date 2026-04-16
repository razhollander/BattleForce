using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService
{
    public interface IPlayersDecelerationLogic
    {
        void DeceleratePlayerVelocity(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTIme);
        void DeceleratePlayerSpin(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTime);
    }
}
