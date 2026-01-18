using System.Collections.Generic;
using System.Numerics;
using Box2D.NetStandard.Dynamics.Bodies;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public interface IPhysicsSimulator
    {
        void InitEntryPoint();
        void Step(float deltaTime, int velocityIterations, int positionIterations);
        void SetPlayerVelocity(ushort playerId, Vector2 velocity);
        void AddWall(ushort id, Vector2[] points);
        void AddLava(ushort id, Vector2[] points);
        void AddPlayer(ushort id, ushort teamId, Vector2 position, Vector2 velocity, float radius);
        public FixedUnorderedList<PhysicsCollisionEvent> GetCachedCollisions();
        public void ClearCachedCollisions();
        void InitExitPoint();
        void CopyDataToSimulation(SimulationStateS2C simulationState);
        Body GetPlayer(ushort playerId);
        void AddPlayerBullet(ushort bulletId, ushort teamId, Vector2 bulletPosition, Vector2 bulletVelocity, float bulletRadius);
        void AddTalentCard(ushort id, Vector2 position, float length, float height);
        Body GetBullet(ushort bulletId);
        void RemoveBody(Body body);
    }
}