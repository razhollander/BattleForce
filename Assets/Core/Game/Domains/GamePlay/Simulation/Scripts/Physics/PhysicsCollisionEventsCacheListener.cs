using System;
using System.Collections.Generic;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public class CollisionEventCacheListener : ContactListener
    {
        private readonly FixedUnorderedList<PhysicsCollisionEvent> _events;
        private readonly HashSet<EventKey> _noneEqualEvents;

        public CollisionEventCacheListener(NetworkConfig networkConfig)
        {
            int maxCapMaxCollisionsPerFrame = networkConfig.MaxCap.MaxCollisionsPerFrame;
            _events = new FixedUnorderedList<PhysicsCollisionEvent>(maxCapMaxCollisionsPerFrame);

            for (int i = 0; i < maxCapMaxCollisionsPerFrame; i++)
            {
                ref var physicsCollisionEvent = ref _events.AddAndGet();
                physicsCollisionEvent = new PhysicsCollisionEvent();
            }
            _events.Clear();
            _noneEqualEvents = new HashSet<EventKey>(maxCapMaxCollisionsPerFrame);
        }

        /// <summary>
        /// Cached events since last Clear().
        /// </summary>
        public FixedUnorderedList<PhysicsCollisionEvent> Events => _events;
    
        public void Clear()
        {
            _events.Clear();
            _noneEqualEvents.Clear();
        }

        public override void BeginContact(in Contact contact) => Cache(PhysicsEventEventType.Begin, contact);
        public override void EndContact(in Contact contact) => Cache(PhysicsEventEventType.End, contact);

        public override void PreSolve(in Contact contact, in Manifold oldManifold)
        {
        
        }

        public override void PostSolve(in Contact contact, in ContactImpulse impulse)
        {
        
        }

        private void Cache(PhysicsEventEventType type, Contact contact)
        {
            var fixtureA = contact.GetFixtureA();
            var fixtureB = contact.GetFixtureB();
            
            var bodyDataA = (PhysicsBodyData)fixtureA.Body.UserData;
            var bodyDataB = (PhysicsBodyData)fixtureB.Body.UserData;
            var velocityA = fixtureA.GetBody().GetLinearVelocity();
            var velocityB = fixtureB.GetBody().GetLinearVelocity();
            
            var key = new EventKey(bodyDataA, bodyDataB, type);
            if (_noneEqualEvents.Add(key))
            {
                ref var physicsCollisionEvent = ref _events.AddAndGet();
                physicsCollisionEvent.Contact = contact;
                physicsCollisionEvent.Type = type;
                physicsCollisionEvent.BodyDataA = bodyDataA;
                physicsCollisionEvent.BodyDataB = bodyDataB;
                physicsCollisionEvent.FixtureA = fixtureA;
                physicsCollisionEvent.FixtureB = fixtureB; 
                physicsCollisionEvent.VelocityA = velocityA; 
                physicsCollisionEvent.VelocityB = velocityB; 
            }
            else
            {
                LogService.LogError($"Already have event {key}");
            }
        }
        
        private readonly struct EventKey : IEquatable<EventKey>
        {
            private readonly ushort _lowId;
            private readonly ushort _highId;
            private readonly PhysicsBodyType _lowType;
            private readonly PhysicsBodyType _highType;
            private readonly PhysicsEventEventType _type;

            public EventKey(PhysicsBodyData bodyA, PhysicsBodyData bodyB, PhysicsEventEventType type)
            {
                bool aIsLow;
                if (bodyA.Id < bodyB.Id)
                {
                    aIsLow = true;
                }
                else if (bodyA.Id > bodyB.Id)
                {
                    aIsLow = false;
                }
                else
                {
                    // IDs equal, compare types to ensure deterministic order
                    aIsLow = bodyA.PhysicsBodyType <= bodyB.PhysicsBodyType;
                }

                if (aIsLow)
                {
                    _lowId = bodyA.Id;
                    _lowType = bodyA.PhysicsBodyType;
                    _highId = bodyB.Id;
                    _highType = bodyB.PhysicsBodyType;
                }
                else
                {
                    _lowId = bodyB.Id;
                    _lowType = bodyB.PhysicsBodyType;
                    _highId = bodyA.Id;
                    _highType = bodyA.PhysicsBodyType;
                }

                _type = type;
            }

            public bool Equals(EventKey other)
                => _lowId == other._lowId && _highId == other._highId &&
                   _lowType == other._lowType && _highType == other._highType &&
                   _type == other._type;

            public override bool Equals(object obj) => obj is EventKey other && Equals(other);

            public override string ToString()
            {
                return $"Low: ({_lowType}:{_lowId}), High: ({_highType}:{_highId}), Type: {_type}";
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = 17;
                    h = (h * 31) + _lowId;
                    h = (h * 31) + _highId;
                    h = (h * 31) + (int)_lowType;
                    h = (h * 31) + (int)_highType;
                    h = (h * 31) + (int)_type;
                    return h;
                }
            }
        }
    }
}