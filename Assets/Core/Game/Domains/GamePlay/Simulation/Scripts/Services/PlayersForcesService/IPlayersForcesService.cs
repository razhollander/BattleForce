using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService
{
    public interface IPlayersForcesService
    {
        void AddForce(ushort playerId, Vector2 forcePower, float acceleration, float spinPower, float spinAcceleration);
        Vector2 CalculatePlayerVelocity(ushort playerId);
        float CalculatePlayerSpin(ushort playerId);
        void Tick(float deltaTime);
        void Clear(ushort playerId);
    }
}
