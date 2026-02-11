using System.Numerics;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services
{
    public interface IPhysicsCasterService
    {
        int CreateCircleCaster(CastType type, Vector2 position, float radius);
        int CreateRectangleCaster(CastType type, Vector2 position, float width, float height, Vector2 rotation);
        void ClearResults();
        void ClearCasters();
        void CacheCollision(ushort casterId, PhysicsBodyData otherBody, PhysicsEventEventType eventType, Contact contact);
        FixedUnorderedList<CasterCollisionInfo> GetCastCollisionsOfCast(int castGui);
    }

    public struct CasterCollisionInfo
    {
        public PhysicsBodyData HitBody;
        public PhysicsEventEventType Type;
        // Add more fields if needed
    }
}