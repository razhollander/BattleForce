using System;
using System.Collections.Generic;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

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
            var idA = bodyDataA.Id;
            var idB = bodyDataB.Id;
            var velocityA = fixtureA.GetBody().GetLinearVelocity();
            var velocityB = fixtureB.GetBody().GetLinearVelocity();
            
            var key = new EventKey(idA, idB, type);
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
        }
        
        private readonly struct EventKey : IEquatable<EventKey>
        {
            private readonly int _lowId;
            private readonly int _highId;
            private readonly PhysicsEventEventType _type;

            public EventKey(int idA, int idB, PhysicsEventEventType type)
            {
                if (idA <= idB) { _lowId = idA; _highId = idB; }
                else { _lowId = idB; _highId = idA; }

                _type = type;
            }

            public bool Equals(EventKey other)
                => _lowId == other._lowId && _highId == other._highId && _type == other._type;

            public override bool Equals(object obj) => obj is EventKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = 17;
                    h = (h * 31) + _lowId;
                    h = (h * 31) + _highId;
                    h = (h * 31) + (int)_type;
                    return h;
                }
            }
        }
    }
}