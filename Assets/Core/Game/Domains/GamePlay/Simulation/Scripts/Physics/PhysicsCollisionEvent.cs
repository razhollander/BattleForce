using System.Numerics;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public class PhysicsCollisionEvent
    {
        public PhysicsEventEventType Type;
        public PhysicsBodyData BodyDataA;
        public PhysicsBodyData BodyDataB;
        public Fixture FixtureA;
        public Fixture FixtureB;
        public Vector2 VelocityA;
        public Vector2 VelocityB;
        public Contact Contact;

        public override string ToString() => $"{Type}: {FixtureA} <-> {FixtureB}";
    }
}