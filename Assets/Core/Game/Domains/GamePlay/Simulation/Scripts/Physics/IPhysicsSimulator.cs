using System.Collections.Generic;
using System.Numerics;
using Box2D.NetStandard.Dynamics.Bodies;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public interface IPhysicsSimulator
    {
        void InitEntryPoint();
        void Step(float deltaTime, int velocityIterations, int positionIterations);
        void SetPlayerVelocity(ushort playerId, Vector2 velocity);
        void AddWall(ushort id, Vector2[] points, Vector2 position);
        void AddLavaWall(ushort id, Vector2[] points, Vector2 position);
        void AddStageBoundary(ushort id, Vector2[] points, Vector2 position);
        void AddTeamFloor(ushort id, Vector2[] points, Vector2 position);
        void AddPlayer(ushort id, ushort teamId, Vector2 position, Vector2 velocity, float radius, float heartRadius);
        public FixedUnorderedList<PhysicsCollisionEvent> GetCachedCollisions();
        public void ClearCachedCollisions();
        void InitExitPoint();
        void CopyDataToSimulation(MatchSimulationStateS2C simulationState, FixedClassUnorderedList<EnvironmentWallS2C> environmentWalls, FixedClassUnorderedList<EnvironmentWallS2C> environmentLavaWalls, FixedClassUnorderedList<EnvironmentSpringS2C> environmentSprings, FixedClassUnorderedList<EnvironmentTeleportGatePairS2C> environmentTeleportGates);
        void CopyDataToSimulation(MatchMakingSimulationStateS2C simulationState);
        Body GetPlayer(ushort playerId);
        Body GetKOProjectile(ushort koProjectileId);
        Body GetGrapplingHookProjectile(ushort grapplingHookProjectileId);
        void AddPlayerBullet(ushort bulletId, ushort teamId, Vector2 bulletPosition, Vector2 bulletVelocity, float bulletRadius);
        void AddTalentCard(ushort id, Vector2 position, float length, float height);
        void AddPowerUpBall(ushort id, Vector2 position, Vector2 velocity, float radius);
        Body GetBullet(ushort bulletId);
        Body GetPowerUpBall(ushort powerUpBallId);
        void RemoveBody(Body body);
        bool IsSquareHitAnyBodyTypes(Vector2 squarePosition, float squareHalfWidth, params PhysicsBodyType[] bodyTypes);
        bool CircleCast(Vector2 center, float radius, params PhysicsBodyType[] bodyTypes);
        bool RectangleCast(Vector2 center, Vector2 size, float angleRadians, params PhysicsBodyType[] bodyTypes);
        void AddStartMatchWall(ushort id, Vector2 position, float radius);
        void AddEnvironmentSpring(ushort id, Vector2 position, float rotationDegrees, Vector2 size);
        void AddTeleportGate(ushort id, Vector2 position, float rotation, Vector2 size);
        void AddSwapField(ushort id, ushort teamId, Vector2 position);
        void RemoveSwapField(ushort id);
        void AddKOProjectile(ushort id, ushort teamId, Vector2 position, float radius, Vector2 velocity);
        void RemoveKOProjectile(ushort id);
        void AddGrapplingHookProjectile(ushort id, ushort teamId, Vector2 position, float radius, Vector2 velocity);
        void UpdateGrapplingHookProjectile(ushort id, Vector2 position, Vector2 velocity);
        void RemoveGrapplingHookProjectile(ushort id);
        void ClearAllData();
        void DisableBodyCollider(PhysicsBodyType koProjectile, ushort projectileId);
        bool ArcCastOnPlayers(Vector2 center, float radius, Vector2 directon, float arcAngleDegrees, short ingoredTeamId, out PhysicsBodyData hitBodyData);
        void AddChickenEgg(ushort eggId, ushort teamId, Vector2 position, float eggRadius);
        Body GetChickenEgg(ushort chieckEggId);
        void RemoveChickenEgg(ushort eggId);
        bool RectangleCastOnPlayers(Vector2 center, Vector2 size, float angleRadians, short ignoreTeamId, out PhysicsBodyData hitBodyData);
    }
}