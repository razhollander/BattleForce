using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService
{
    public interface IPlayersEngineLogic
    {
        void TurnOnEngineForPlayerIfPossible(PlayerStateS2C playerState);
        void TryAddEngineForceToPlayer(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTIme);
    }
}