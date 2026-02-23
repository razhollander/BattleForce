using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepAllWheelsRotationCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private int _tick;
        private float _deltaTime;

        public StepAllWheelsRotationCommand SetTime(int tick, float deltaTime)
        {
            _tick = tick;
            _deltaTime = deltaTime;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
        }

        public void Execute()
        {
            foreach (var rotatingWheel in _matchDataService.Environment.RotatingWheels)
            {
                UpdateRotationAccordingToTick(_tick, _deltaTime, rotatingWheel);
            }
        }

        private void UpdateRotationAccordingToTick(int tick, float deltaTime, EnvironmentRotatingWheelConfig rotatingWheelConfig)
        {
            var rotationSpeed = rotatingWheelConfig.RotationSpeed;
            var wheelCenter = rotatingWheelConfig.CenterPosition;

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
                        0,
                        out var newPos,
                        out var newRot
                    );

                    wall.WorldRotationAngle = newRot;
                    wall.WorldPosition = newPos;
                    _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.Wall, wall.Id, newPos, newRot);
                }
            }

            if (rotatingWheelConfig.LavaWalls != null)
            {
                foreach (var lavaWall in rotatingWheelConfig.LavaWalls)
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

                    lavaWall.WorldRotationAngle = newRot;
                    lavaWall.WorldPosition = newPos;
                    _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.Lava, lavaWall.Id, newPos, newRot);
                }
            }

            if (rotatingWheelConfig.Springs != null)
            {
                foreach (var spring in rotatingWheelConfig.Springs) // todo for ai, create new SpringState/WallState and use them instead of using and updating the configs
                {
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        spring.Position,
                        spring.RotationAngle,
                        out var newPos,
                        out var newRot
                    );

                    spring.WorldRotationAngle = newRot;
                    spring.WorldPosition = newPos;
                    _physicsSimulator.UpdateBodyTransform(PhysicsBodyType.EnvironmentSpring, spring.Id, newPos, newRot);
                }
            }
        }
    }
}