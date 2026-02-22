using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.EnvironmentRotatingWheel
{
    public class EnvironmentRotatingWheelControllers : IEnvironmentRotatingWheelControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly IPhysicsSimulator _physicsSimulator;

        public EnvironmentRotatingWheelControllers(IMatchDataService matchDataService, IPhysicsSimulator physicsSimulator)
        {
            _matchDataService = matchDataService;
            _physicsSimulator = physicsSimulator;
        }

        public void StepAllWheelsRotation(int tick, float deltaTime)
        {
            foreach (var rotatingWheel in _matchDataService.Environment.RotatingWheels)
            {
                UpdateRotationAccordingToTick(tick, deltaTime, rotatingWheel);
            }
        }
        
        private void UpdateRotationAccordingToTick(int tick, float deltaTime, EnvironmentRotatingWheelConfig rotatingWheelConfig)
        {
            var rotationSpeed = rotatingWheelConfig.RotationSpeed;
            var wheelCenter = rotatingWheelConfig.CenterPosition;

            // Walls (Assuming walls are centered at (0,0) relative to wheel)
            if (rotatingWheelConfig.Walls != null)
            {
                foreach (var wall in rotatingWheelConfig.Walls)
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
            if (rotatingWheelConfig.LavaWalls != null)
            {
                foreach (var wall in rotatingWheelConfig.LavaWalls)
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
            if (rotatingWheelConfig.Springs != null)
            {
                foreach (var spring in rotatingWheelConfig.Springs)
                {
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        spring.Position,
                        spring.RotationAngle, // Springs have a direction
                        out var newPos,
                        out var newRot
                    );

                    spring.WorldRotationAngle = newRot;
                    _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.EnvironmentSpring, spring.Id, newPos, newRot);
                }
            }
        }
    }
}