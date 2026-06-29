using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class ProvideNormalForceToPlayerStickWithWallCommand : BaseCommand, ICommandVoid
    {
        // For how many ticks a player must keep touching a wall before we start cancelling the velocity it pushes into it.
        private const int STICK_TO_WALL_TICKS_THRESHOLD = 6;

        private IMatchDataService _matchDataService;
        private IPlayersTouchingWallDataService _playersTouchingWallDataService;
        private ICommandFactory _commandFactory;
        private AddForceToPlayerCommand _addForceToPlayerCommand;

        private int _tick;

        public ProvideNormalForceToPlayerStickWithWallCommand SetTick(int tick)
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
            var playersStickToWall = _playersTouchingWallDataService.GetPlayersStickToWall(_tick, STICK_TO_WALL_TICKS_THRESHOLD);

            for (int i = 0; i < playersStickToWall.Count; i++)
            {
                var stickData = playersStickToWall[i];
                var playerState = _matchDataService.SimulationState.GetPlayerById(stickData.PlayerId);
                var velocity = playerState.Spaceship.Transform.Velocity;
                var wallNormal = stickData.WallNormal;

                if (!velocity.IsFacingWall(wallNormal))
                {
                    continue;
                }

                // Cancel exactly the velocity component heading into the wall, leaving the velocity along the wall untouched.
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
