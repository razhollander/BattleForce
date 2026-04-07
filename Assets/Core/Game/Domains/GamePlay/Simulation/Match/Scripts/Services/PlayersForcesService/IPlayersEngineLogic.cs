using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService
{
    public interface IPlayersEngineLogic
    {
        void TurnOnEngineIfPlayerIdle(PlayerSpaceshipStateS2C playerSpaceshipState);
        void TryAddEngineForceToPlayer(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTIme);
    }
}