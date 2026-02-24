using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;
using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class EnforceFieldBarriersCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private INetEventsDataService _netEventsDataService;
        private int _tick;

        public EnforceFieldBarriersCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var barriers = _matchDataService.EnvironmentData.FieldBarriers;
            if (barriers.Count == 0) return;

            // Enforce Players
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!player.Spaceship.IsAlive) continue;

                var barrier = GetBarrierForTeam(player.TeamId);
                if (barrier == null) continue;

                EnforcePlayerBarrier(player, barrier);
            }

            // Enforce Bullets
            var bullets = _matchDataService.SimulationState.Bullets;
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                ref var bullet = ref bullets.GetByIndex(i);

                // Assuming player exists if bullet exists. If not, we might skip enforcement or just destroy.
                // But we need TeamID.
                // Bullet has BelongToPlayerId.
                // We can iterate players to find team.

                // Optimize: Map PlayerId -> TeamId? Or just lookup.
                // SimulationState.GetPlayerById throws if not found? No, it returns reference or default?
                // SimulationState.Players.FindWithId returns reference.
                // If player disconnected, they might be removed?
                // MatchSimulationStateS2C.Players is FixedUnorderedList.
                // If player is removed, bullet should probably be destroyed anyway, but let's see.

                // If TryGetPlayerById fails, we skip enforcement (or destroy).
                // Existing code:
                /*
                public PlayerStateS2C GetPlayerById(ushort playerId)
                {
                    return Players.FindWithId(playerId);
                }
                */
                // FindWithId usually iterates.

                // Let's try get player.
                // We should check if player exists.
                // FixedUnorderedList doesn't have TryFindWithId easily exposed without iteration.
                // But GetPlayerById might throw or return null/default if not found?
                // Based on `ProcessCachedCollisionsCommand`, it seems safe to assume player exists or we handle it.
                // But wait, if player disconnects, are they removed from SimulationState immediately?
                // If so, we can't find team.
                // If we can't find team, we can't enforce barrier.
                // Maybe destroy bullet if player not found?

                // I'll iterate players manually to avoid exception if GetPlayerById throws.
                var playerId = bullet.BelongToPlayerId;
                ushort teamId = 0;
                bool playerFound = false;

                foreach(var p in _matchDataService.SimulationState.Players.AsSpan())
                {
                    if (p.Id == playerId)
                    {
                        teamId = p.TeamId;
                        playerFound = true;
                        break;
                    }
                }

                if (!playerFound) continue; // Skip if player not found (bullet might be orphaned, logic elsewhere handles it or it stays).

                var barrier = GetBarrierForTeam(teamId);
                if (barrier == null) continue;

                if (!IsPointInsideBarrier(bullet.Position, barrier))
                {
                    DestroyBullet(ref bullet);
                    bullets.RemoveAt(i);
                }
            }
        }

        private MatchEnvironmentFieldBarrierModel GetBarrierForTeam(ushort teamId)
        {
            foreach (var barrier in _matchDataService.EnvironmentData.FieldBarriers.AsSpan())
            {
                if (barrier.TeamId == teamId) return barrier;
            }
            return null;
        }

        private void EnforcePlayerBarrier(Core.Game.Domains.GamePlay.Shared.S2CModels.PlayerStateS2C player, MatchEnvironmentFieldBarrierModel barrier)
        {
            var position = player.Spaceship.Transform.Position;
            if (IsPointInsideBarrier(position, barrier)) return;

            // Clamp position
            if (barrier.Shape == FieldBarrierShape.Circle)
            {
                var center = barrier.Position;
                var radius = barrier.Size.X;
                var direction = position - center;
                if (direction.LengthSquared() > radius * radius)
                {
                    player.Spaceship.Transform.Position = center + Vector2.Normalize(direction) * radius;
                }
            }
            else if (barrier.Shape == FieldBarrierShape.Rectangle)
            {
                var center = barrier.Position;
                var halfSize = barrier.Size * 0.5f;
                var min = center - halfSize;
                var max = center + halfSize;

                var clampedX = Math.Clamp(position.X, min.X, max.X);
                var clampedY = Math.Clamp(position.Y, min.Y, max.Y);
                player.Spaceship.Transform.Position = new Vector2(clampedX, clampedY);
            }
        }

        private bool IsPointInsideBarrier(Vector2 point, MatchEnvironmentFieldBarrierModel barrier)
        {
             if (barrier.Shape == FieldBarrierShape.Circle)
            {
                var center = barrier.Position;
                var radius = barrier.Size.X;
                return Vector2.DistanceSquared(point, center) <= radius * radius;
            }
            else if (barrier.Shape == FieldBarrierShape.Rectangle)
            {
                var center = barrier.Position;
                var halfSize = barrier.Size * 0.5f;
                var min = center - halfSize;
                var max = center + halfSize;
                return point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;
            }
            return false;
        }

        private void DestroyBullet(ref Core.Game.Domains.GamePlay.Shared.S2CModels.PlayerBulletS2C bullet)
        {
            var body = _physicsSimulator.GetBullet(bullet.Id);
            if (body != null)
            {
                _physicsSimulator.RemoveBody(body);
            }
            _netEventsDataService.AddBulletDestroyedNetEvent(_tick, bullet.Id, bullet.Position);
        }
    }
}
