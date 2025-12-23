using System;
using System.Collections.Generic;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;

public enum PhysicsEventEventType
{
    Begin,
    End
}

public class CollisionEventCacheListener : ContactListener
{
    private readonly struct EventKey : IEquatable<EventKey>
    {
        private readonly int _low;
        private readonly int _high;
        private readonly PhysicsEventEventType _type;

        public EventKey(int idA, int idB, PhysicsEventEventType type)
        {
            if (idA <= idB) { _low = idA; _high = idB; }
            else { _low = idB; _high = idA; }

            _type = type;
        }

        public bool Equals(EventKey other)
            => _low == other._low && _high == other._high && _type == other._type;

        public override bool Equals(object obj) => obj is EventKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = (h * 31) + _low;
                h = (h * 31) + _high;
                h = (h * 31) + (int)_type;
                return h;
            }
        }
    }

    private readonly List<PhysicsCollisionEvent> _events = new List<PhysicsCollisionEvent>(256);
    private readonly HashSet<EventKey> _seen = new HashSet<EventKey>();

    // If your fixtures don't expose stable IDs, we assign them based on reference identity.
    private readonly Dictionary<object, int> _ids =
        new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
    private int _nextId = 1;

    /// <summary>
    /// Cached events since last Clear().
    /// </summary>
    public IReadOnlyList<PhysicsCollisionEvent> Events => _events;
    
    public void Clear()
    {
        _events.Clear();
        _seen.Clear();
        // Note: we intentionally do NOT clear _ids so fixture IDs stay stable across frames.
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
        // Adjust based on your port:
        // Some ports: contact.GetFixtureA()/GetFixtureB()
        // Others: contact.FixtureA / contact.FixtureB
        Fixture a = contact.GetFixtureA();
        Fixture b = contact.GetFixtureB();
        int idA = GetId(a);
        int idB = GetId(b);

        var key = new EventKey(idA, idB, type);
        if (_seen.Add(key))
            _events.Add(new PhysicsCollisionEvent(type, a, b, contact));
    }

    private int GetId(object obj)
    {
        if (obj == null) return 0;

        if (_ids.TryGetValue(obj, out int id))
            return id;

        id = _nextId++;
        _ids[obj] = id;
        return id;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

        public new bool Equals(object x, object y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj)
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
