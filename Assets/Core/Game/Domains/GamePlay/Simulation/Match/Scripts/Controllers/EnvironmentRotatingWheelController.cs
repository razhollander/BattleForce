using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Controllers
{
    public class EnvironmentRotatingWheelController
    {
        private readonly EnvironmentRotatingWheelConfig _config;
        private readonly IPhysicsSimulator _physicsSimulator;

        public EnvironmentRotatingWheelController(EnvironmentRotatingWheelConfig config, IPhysicsSimulator physicsSimulator)
        {
            _config = config;
            _physicsSimulator = physicsSimulator;
        }

        public void UpdateRotationAccordingToTick(int tick, float deltaTime)
        {
            var rotationSpeed = _config.RotationSpeed;
            var wheelCenter = _config.CenterPosition;

            // Walls (Assuming walls are centered at (0,0) relative to wheel)
            if (_config.Walls != null)
            {
                foreach (var wall in _config.Walls)
                {
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        Vector2.Zero,
                        0, // Wall Config points are rotated by the wheel rotation
                        out var newPos,
                        out var newRot
                    );
                    _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.Wall, wall.Id, newPos, newRot);
                }
            }

            // Lava Walls
            if (_config.LavaWalls != null)
            {
                 foreach (var wall in _config.LavaWalls)
                {
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        Vector2.Zero,
                        0,
                        out var newPos,
                        out var newRot
                    );
                    _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.Lava, wall.Id, newPos, newRot);
                }
            }

            // Springs
            if (_config.Springs != null)
            {
                foreach (var spring in _config.Springs)
                {
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        spring.Position,
                        spring.DirectionAngle, // Springs have a direction
                        out var newPos,
                        out var newRot
                    );
                    _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.EnvironmentSpring, spring.Id, newPos, newRot);
                }
            }
        }
    }
}
