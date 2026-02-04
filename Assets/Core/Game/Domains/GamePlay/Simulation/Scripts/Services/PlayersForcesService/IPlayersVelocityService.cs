using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService
{
    public interface IPlayersVelocityService
    {
        void AddForceToPlayer(PlayerSpaceshipStateS2C playerSpaceshipState, Vector2 force);
        void AddSpinToPlayer(PlayerSpaceshipStateS2C playerSpaceshipState, float spin);
        void StepPlayerVelocity(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTIme);
        void StepPlayerSpin(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTime);
    }
}
