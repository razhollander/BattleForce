using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class EnforceStageBarriersCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPlayersOutsideStageTrackerService _playersOutsideStageTrackerService;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersOutsideStageTrackerService = _diContainer.Resolve<IPlayersOutsideStageTrackerService>();
        }

        public void Execute()
        {
            // todo: also destory any projectile outside of stage barriers OR somehow always keep the players inside the stage boundries and the positions passed in CreateXProjectilesNetEvent 
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var playerId = playerState.Id;
                if (_playersOutsideStageTrackerService.IsPlayerOutside(playerId))
                {
                    SnapPlayerToClosestBoundaryPoint(playerState);
                }
            }
        }

        private void SnapPlayerToClosestBoundaryPoint(PlayerStateS2C playerState)
        {
            var playerPosition = playerState.Spaceship.Transform.Position;
            var playerRadius = playerState.Spaceship.Transform.Radius;
            var stageBoundaries = _matchDataService.EnvironmentData.StageBoundaries;
            var minDistanceSq = float.MaxValue;
            var closestPointOnAnyBoundary = playerPosition;
            var bestNormal = Vector2.Zero;

            for (int i = 0; i < stageBoundaries.Count; i++)
            {
                var boundary = stageBoundaries.GetByIndex(i);
                var points = boundary.Points;
                var validPointCount = boundary.PointsCount;
                if (validPointCount < 3) continue;

                var worldPos = boundary.Transform.WorldPosition;

                for (int j = 0; j < validPointCount; j++)
                {
                    var p1 = points[j] + worldPos;
                    var p2 = points[(j + 1) % validPointCount] + worldPos;

                    var closestPointOnEdge = MathUtils.GetClosestPointOnSegment(p1, p2, playerPosition);
                    var distSq = Vector2.DistanceSquared(playerPosition, closestPointOnEdge);
                    var isCurrentClosestPoint = distSq < minDistanceSq;

                    if (!isCurrentClosestPoint)
                    {
                        continue;
                    }

                    minDistanceSq = distSq;
                    closestPointOnAnyBoundary = closestPointOnEdge;

                    var edgeDir = (p2 - p1).NormalizeSafe();
                    bestNormal = new Vector2(-edgeDir.Y, edgeDir.X);
                }
            }

            // Box2D strictly uses CCW winding.
            // So if p1 -> p2 is the edge, the inward normal is (-edgeDir.Y, edgeDir.X).
            var pushVector = bestNormal;
            var newPos = closestPointOnAnyBoundary + pushVector * playerRadius;
            playerState.Spaceship.Transform.Position = newPos;
        }
    }
}
