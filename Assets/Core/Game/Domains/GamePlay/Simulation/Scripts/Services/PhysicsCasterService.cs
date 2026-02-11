using System.Numerics;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services
{
    public class PhysicsCasterService : IPhysicsCasterService
    {
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;

        private ushort _nextCasterId;
        private int _nextGuiId;

        // Map casterId (ushort) -> guiId (int)
        private readonly CapacityDict<ushort, int> _casterIdToGui;

        // Map guiId (int) -> List of Collisions
        private readonly CapacityDict<int, FixedUnorderedList<CasterCollisionInfo>> _guiToCollisions;

        // List of bodies to clear
        private readonly FixedUnorderedList<Body> _activeBodies;

        // Pool for collision lists
        private readonly ConcurrentPool<FixedUnorderedList<CasterCollisionInfo>> _collisionListPool;

        public PhysicsCasterService(IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig)
        {
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;

            // Reasonable max per frame
            int maxCasters = 200;
            _casterIdToGui = new CapacityDict<ushort, int>(maxCasters);
            _guiToCollisions = new CapacityDict<int, FixedUnorderedList<CasterCollisionInfo>>(maxCasters);
            _activeBodies = new FixedUnorderedList<Body>(maxCasters);

            _collisionListPool = new ConcurrentPool<FixedUnorderedList<CasterCollisionInfo>>(
                () => new FixedUnorderedList<CasterCollisionInfo>(64),
                maxCasters);

            _nextGuiId = 1000;
        }

        public int CreateCircleCaster(CastType type, Vector2 position, float radius)
        {
            var id = _nextCasterId++;
            var gui = _nextGuiId++;

            _casterIdToGui.Add(id, gui);

            var list = _collisionListPool.Get();
            list.Clear();
            _guiToCollisions.Add(gui, list);

            var body = _physicsSimulator.AddCaster(id, position, radius);
            ref var bodyRef = ref _activeBodies.AddAndGet();
            bodyRef = body;

            return gui;
        }

        public int CreateRectangleCaster(CastType type, Vector2 position, float width, float height, Vector2 rotation)
        {
            var id = _nextCasterId++;
            var gui = _nextGuiId++;

            _casterIdToGui.Add(id, gui);

            var list = _collisionListPool.Get();
            list.Clear();
            _guiToCollisions.Add(gui, list);

            var body = _physicsSimulator.AddCaster(id, position, width, height, rotation);
            ref var bodyRef = ref _activeBodies.AddAndGet();
            bodyRef = body;

            return gui;
        }

        public void ClearCasters()
        {
            for (int i = 0; i < _activeBodies.Count; i++)
            {
                _physicsSimulator.RemoveBody(_activeBodies[i]);
            }
            _activeBodies.Clear();

            _nextCasterId = 0;
            _casterIdToGui.Clear();
        }

        public void ClearResults()
        {
            foreach (var kvp in _guiToCollisions)
            {
                var list = kvp.Value;
                list.Clear();
                _collisionListPool.Return(list);
            }
            _guiToCollisions.Clear();
        }

        public void CacheCollision(ushort casterId, PhysicsBodyData otherBody, PhysicsEventEventType eventType, Contact contact)
        {
            if (!_casterIdToGui.TryGetValue(casterId, out var gui))
            {
                return;
            }

            if (_guiToCollisions.TryGetValue(gui, out var list))
            {
                if (list.IsFull)
                {
                    LogService.LogError($"Collision list full for caster {gui}");
                    return;
                }

                ref var info = ref list.AddAndGet();
                info.HitBody = otherBody;
                info.Type = eventType;
            }
        }

        public FixedUnorderedList<CasterCollisionInfo> GetCastCollisionsOfCast(int castGui)
        {
            if (_guiToCollisions.TryGetValue(castGui, out var list))
            {
                return list;
            }
            return null;
        }
    }
}
