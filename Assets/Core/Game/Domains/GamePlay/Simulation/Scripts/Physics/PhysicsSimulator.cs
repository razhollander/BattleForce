using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public class PhysicsSimulator
    {
        private World _world;

        public void InitEntryPoint()
        {
            _world = CreateWorld();
        }
        
        public void Step(float deltaTime, int velocityIterations, int positionIterations)
        {
            _world.Step(deltaTime, velocityIterations, positionIterations);
        }
        
        private World CreateWorld()
        {
            var gravity = new Vector2(0f, 0f);
            var world = new World(gravity);

            return world;
        }

        public void SetPlayerVelocity(int playerId, Vector2 velocity)
        {
            var currentBody = _world.GetBodyList();

            while (currentBody != null)
            {
                var bodyData = (PhysicsBodyData)currentBody.UserData;
                bool isPlayer = bodyData.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && bodyData.Id == playerId;
                if (isPlayer)
                {
                    currentBody.SetLinearVelocity(velocity);
                    return;
                }

                currentBody = currentBody.GetNext();
            }
        }

        public void AddWall(int id, Vector2[] points)
        {
            BodyDef bodyDef = new BodyDef 
            {
                type = BodyType.Static,
                position = Vector2.Zero, // Assume walls are absolute-world positioned
                userData = new PhysicsBodyData(id, PhysicsBodyType.Wall)
            };

            Body body = _world.CreateBody(bodyDef);

            PolygonShape wallShape = new PolygonShape();
            wallShape.Set(points);

            FixtureDef fixtureDef = new FixtureDef
            {
                shape = wallShape,
                density = 0,       // Static objects don't need density
                friction = 0  
            };

            body.CreateFixture(fixtureDef);
        }
        
        public void AddPlayer(int id, Vector2 position, Vector2 velocity, float radius)
        {
            BodyDef bodyDef = new BodyDef
            {
                position = position,
                linearVelocity = velocity,
                type = BodyType.Dynamic,
                userData = new PhysicsBodyData(id, PhysicsBodyType.PlayerSpaceship)
            };

            Body body = _world.CreateBody(bodyDef);

            CircleShape circleShape = new CircleShape();
            circleShape.Radius = radius;

            FixtureDef fixtureDef = new FixtureDef
            {
                shape = circleShape,
                density = 1.0f,
                friction = 0
            };

            body.CreateFixture(fixtureDef);
        }
    }
}