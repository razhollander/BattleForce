using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class AddNormalForceToPlayerStickWithWallCommand : BaseCommand, ICommandVoid
    {
        private const int MAX_STICK_TO_WALL_TICKS_BEFORE_CANCELING_VELOCITY = 4;

        private IMatchDataService _matchDataService;
        private IPlayersTouchingWallDataService _playersTouchingWallDataService;
        private ICommandFactory _commandFactory;
        private AddForceToPlayerCommand _addForceToPlayerCommand;

        private int _tick;

        public AddNormalForceToPlayerStickWithWallCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersTouchingWallDataService = _diContainer.Resolve<IPlayersTouchingWallDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _addForceToPlayerCommand = _commandFactory.CreateCommandVoid<AddForceToPlayerCommand>();
        }

        public void Execute()
        {
            var playersStickToWall = _playersTouchingWallDataService.GetPlayersStickToWall(_tick, MAX_STICK_TO_WALL_TICKS_BEFORE_CANCELING_VELOCITY);

            for (int i = 0; i < playersStickToWall.Count; i++)
            {
                var stickData = playersStickToWall[i];
                var playerState = _matchDataService.SimulationState.GetPlayerById(stickData.PlayerId);
                var velocity = playerState.Spaceship.Transform.Velocity;

                var wallRotationDegrees = _matchDataService.EnvironmentData.GetWall(stickData.WallId).Transform.WorldRotationDegrees;
                var wallNormal = stickData.WallLocalNormal.Rotate(wallRotationDegrees);

                if (!velocity.IsFacingWall(wallNormal))
                {
                    continue;
                }

                var velocityTowardsWall = Vector2.Dot(velocity, wallNormal) * wallNormal;

                _addForceToPlayerCommand
                    .SetPlayerId(stickData.PlayerId)
                    .SetForce(-velocityTowardsWall)
                    .ShouldTurnOffEngine(false)
                    .Execute();
            }
        }
    }
}
