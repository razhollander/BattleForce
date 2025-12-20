using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public interface IPhysicsSimulator
    {
        void InitEntryPoint();
        void Step(float deltaTime, int velocityIterations, int positionIterations);
        void SetPlayerVelocity(int playerId, Vector2 velocity);
        void AddWall(int id, Vector2[] points);
        void AddPlayer(int id, Vector2 position, Vector2 velocity, float radius);
    }
}