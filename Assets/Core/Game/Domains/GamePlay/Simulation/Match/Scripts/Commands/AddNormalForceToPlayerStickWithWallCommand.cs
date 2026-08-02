using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
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
        private TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;

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
            _tryAddForceToPlayerCommand = _commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
        }

        public void Execute()
        {
            var playersStickToWall = _playersTouchingWallDataService.GetPlayersStickToWall(_tick, MAX_STICK_TO_WALL_TICKS_BEFORE_CANCELING_VELOCITY);

            for (int i = 0; i < playersStickToWall.Count; i++)
            {
                var stickData = playersStickToWall[i];
                var playerState = _matchDataService.SimulationState.GetPlayerById(stickData.PlayerId);
                var velocity = playerState.Spaceship.Transform.Velocity;

                if (!TryGetCurrentWallNormal(stickData, out var wallNormal))
                {
                    continue;
                }

                if (!velocity.IsFacingWall(wallNormal))
                {
                    continue;
                }

                var velocityTowardsWall = Vector2.Dot(velocity, wallNormal) * wallNormal;

                _tryAddForceToPlayerCommand
                    .SetPlayerId(stickData.PlayerId)
                    .SetForce(-velocityTowardsWall)
                    .ShouldTurnOffEngine(false)
                    .Execute();
            }
        }

        // The touched body keeps rotating while the player is stuck to it, so the stored local normal is turned back
        // into a world normal with the body's current rotation. A FrigidBlock can be destroyed mid-contact.
        private bool TryGetCurrentWallNormal(in PlayerStickToWallData stickData, out Vector2 wallNormal)
        {
            if (stickData.WallBodyType == PhysicsBodyType.Wall)
            {
                var wallRotationDegrees = _matchDataService.EnvironmentData.GetWall(stickData.WallId).Transform.WorldRotationDegrees;
                wallNormal = stickData.WallLocalNormal.Rotate(wallRotationDegrees);
                return true;
            }

            if (_matchDataService.SimulationState.TryGetFrigidBlockById(stickData.WallId, out var frigidBlock))
            {
                wallNormal = stickData.WallLocalNormal.Rotate(frigidBlock.Rotation.ToAngleDegrees());
                return true;
            }

            wallNormal = Vector2.Zero;
            return false;
        }
    }
}
