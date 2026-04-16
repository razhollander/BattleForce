using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TrySnapPlayersOutsideStageCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPlayersOutsideStageTrackerService _playersOutsideStageTrackerService;

        private Core.Game.Domains.GamePlay.Simulation.Scripts.Physics.IPhysicsSimulator _physicsSimulator;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersOutsideStageTrackerService = _diContainer.Resolve<IPlayersOutsideStageTrackerService>();
            _physicsSimulator = _diContainer.Resolve<Core.Game.Domains.GamePlay.Simulation.Scripts.Physics.IPhysicsSimulator>();
        }

        public void Execute()
        {
            var stageBoundaries = _matchDataService.EnvironmentData.StageBoundaries;
            if (stageBoundaries.IsEmpty)
            {
                return; // If no boundaries exist, ignore snapping logic.
            }

            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!playerState.IsAlive) continue;

                var playerId = playerState.Id;
                if (_playersOutsideStageTrackerService.IsPlayerOutside(playerId))
                {
                    SnapPlayerToClosestBoundaryPoint(playerState);
                }
            }
        }

        private void SnapPlayerToClosestBoundaryPoint(Core.Game.Domains.GamePlay.Shared.S2CModels.PlayerStateS2C playerState)
        {
            var playerPosition = playerState.Spaceship.Transform.Position;
            var playerRadius = playerState.Spaceship.Transform.Radius;

            var stageBoundaries = _matchDataService.EnvironmentData.StageBoundaries;

            float minDistanceSq = float.MaxValue;
            Vector2 closestPointOnAnyBoundary = playerPosition;

            Vector2 bestNormal = Vector2.Zero;

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

                    var closestPointOnEdge = GetClosestPointOnSegment(p1, p2, playerPosition);
                    var distSq = Vector2.DistanceSquared(playerPosition, closestPointOnEdge);

                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        closestPointOnAnyBoundary = closestPointOnEdge;

                        var edgeDir = Vector2.Normalize(p2 - p1);
                        bestNormal = new Vector2(-edgeDir.Y, edgeDir.X);
                    }
                }
            }

            // Box2D strictly uses CCW winding.
            // So if p1 -> p2 is the edge, the inward normal is (-edgeDir.Y, edgeDir.X).
            var pushVector = bestNormal;

            var newPos = closestPointOnAnyBoundary + pushVector * playerRadius;
            playerState.Spaceship.Transform.Position = newPos;

            // Also update the authoritative physics body to prevent desync
            var body = _physicsSimulator.GetPlayer(playerState.Id);
            if (body != null)
            {
                body.SetTransform(newPos, body.GetAngle());
            }
        }

        private Vector2 GetClosestPointOnSegment(Vector2 p1, Vector2 p2, Vector2 point)
        {
            var l2 = Vector2.DistanceSquared(p1, p2);
            if (l2 == 0) return p1;

            var t = Math.Max(0, Math.Min(1, Vector2.Dot(point - p1, p2 - p1) / l2));
            return p1 + t * (p2 - p1);
        }
    }
}
