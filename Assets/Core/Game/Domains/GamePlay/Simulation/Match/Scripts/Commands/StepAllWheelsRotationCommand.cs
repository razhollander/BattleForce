using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepAllWheelsRotationCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
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
        }

        public void Execute()
        {
            foreach (var rotatingWheel in _matchDataService.EnvironmentData.RotatingWheels.AsSpan())
            {
                UpdateRotationAccordingToTick(_tick, _deltaTime, rotatingWheel);
            }
        }

        private void UpdateRotationAccordingToTick(int tick, float deltaTime, EnvironmentRotatingWheelS2C rotatingWheel)
        {
            var rotationSpeed = rotatingWheel.RotationSpeed;
            var wheelCenter = rotatingWheel.CenterPosition;
            if (!rotatingWheel.WallIds.IsEmpty)
            {
                foreach (var wallId in rotatingWheel.WallIds.AsSpan())
                {
                    var wall = _matchDataService.EnvironmentData.GetWall(wallId);
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        wall.Transform.LocalPosition,
                        wall.Transform.LocalRotationDegrees,
                        out var newPosition,
                        out var newRotation
                    );

                    wall.Transform.WorldRotationDegrees = newRotation;
                    wall.Transform.WorldPosition = newPosition;
                }
            }

            if (!rotatingWheel.LavaWallIds.IsEmpty)
            {
                foreach (var lavaWallId in rotatingWheel.LavaWallIds.AsSpan())
                {
                    var lavaWall = _matchDataService.EnvironmentData.GetLavaWall(lavaWallId);
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        lavaWall.Transform.LocalPosition,
                        lavaWall.Transform.LocalRotationDegrees,
                        out var newPosition,
                        out var newRotation
                    );

                    lavaWall.Transform.WorldRotationDegrees = newRotation;
                    lavaWall.Transform.WorldPosition = newPosition;
                }
            }

            if (!rotatingWheel.SpringIds.IsEmpty)
            {
                foreach (var springId in rotatingWheel.SpringIds.AsSpan()) // todo for ai, create new SpringState/WallState and use them instead of using and updating the configs
                {
                    var spring = _matchDataService.EnvironmentData.GetSpring(springId);
                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        tick,
                        rotationSpeed,
                        deltaTime,
                        wheelCenter,
                        spring.Transform.LocalPosition,
                        spring.Transform.LocalRotationDegrees,
                        out var worldPosition,
                        out var newRotation
                    );

                    spring.Transform.WorldRotationDegrees = newRotation;
                    spring.Transform.WorldPosition = worldPosition;
                }
            }
        }
    }
}