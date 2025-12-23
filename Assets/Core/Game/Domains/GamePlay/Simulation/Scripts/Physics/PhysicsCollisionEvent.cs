using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;

public readonly struct PhysicsCollisionEvent
{
    public readonly PhysicsEventEventType Type;
    public readonly Fixture FixtureA;
    public readonly Fixture FixtureB;
    public readonly Contact Contact;

    public PhysicsCollisionEvent(PhysicsEventEventType type, Fixture a, Fixture b, Contact contact)
    {
        Type = type;
        FixtureA = a;
        FixtureB = b;
        Contact = contact;
    }

    public override string ToString() => $"{Type}: {FixtureA} <-> {FixtureB}";
}