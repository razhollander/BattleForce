using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;
using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;

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
            if (barriers.IsEmpty)
            {
                return;
            }
            
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                var barrier = GetBarrierForTeam(player.TeamId);
                EnforcePlayerBarrier(player, barrier);
            }
            
            var bullets = _matchDataService.SimulationState.Bullets;
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                ref var bullet = ref bullets.GetByIndex(i);
                var playerId = bullet.BelongToPlayerId;
                var teamId = _matchDataService.SimulationState.GetPlayerById(playerId).TeamId;
                var barrier = GetBarrierForTeam(teamId);

                if (!barrier.IsCircleInsideBarrier(bullet.Position, bullet.Radius))
                {
                    DestroyBullet(ref bullet);
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

        private void EnforcePlayerBarrier(PlayerStateS2C player, MatchEnvironmentFieldBarrierModel barrier)
        {
            var playerPosition = player.Spaceship.Transform.Position;
            var playerRadius = player.Spaceship.Transform.Radius;
            
            switch (barrier.Shape)
            {
                case FieldBarrierShape.Circle:
                {
                    var barrierPosition = barrier.Position;
                    var barrierRadius = barrier.Size.X;
                    var maxAllowedDistance = Math.Max(0, barrierRadius - playerRadius); // Math.Max prevents negative distance if the barrier shrinks smaller than the player
                    var offsetVector = playerPosition - barrierPosition;
                    var distanceSquared = offsetVector.LengthSquared();
                    var isPlayerOutsideBarrier = distanceSquared > maxAllowedDistance * maxAllowedDistance;
                    if (isPlayerOutsideBarrier)
                    {
                        player.Spaceship.Transform.Position = barrierPosition + Vector2.Normalize(offsetVector) * maxAllowedDistance;
                    }

                    break;
                }
                case FieldBarrierShape.Rectangle:
                {
                    var center = barrier.Position;
                    var halfSize = barrier.Size * 0.5f;
                    
                    var safeHalfWidth = Math.Max(0, halfSize.X - playerRadius); // Ensure min and max don't cross over if the barrier gets extremely small. 
                    var safeHalfHeight = Math.Max(0, halfSize.Y - playerRadius);

                    var minX = center.X - safeHalfWidth;
                    var maxX = center.X + safeHalfWidth;
                    var minY = center.Y - safeHalfHeight;
                    var maxY = center.Y + safeHalfHeight;
                    
                    var isOutsideX = playerPosition.X < minX || playerPosition.X > maxX;
                    var isOutsideY = playerPosition.Y < minY || playerPosition.Y > maxY;
                    var isPlayerOutSideOfBarrier = isOutsideX || isOutsideY;
                    
                    if (isPlayerOutSideOfBarrier)
                    {
                        var clampedX = Math.Clamp(playerPosition.X, minX, maxX);
                        var clampedY = Math.Clamp(playerPosition.Y, minY, maxY);
                        player.Spaceship.Transform.Position = new Vector2(clampedX, clampedY);
                    }

                    break;
                }
            }
        }

        private void DestroyBullet(ref PlayerBulletS2C bullet)
        {
            var body = _physicsSimulator.GetBullet(bullet.Id);
            _physicsSimulator.RemoveBody(body);
            _netEventsDataService.AddBulletDestroyedNetEvent(_tick, bullet.Id, bullet.Position);
            _matchDataService.SimulationState.RemoveBulletById(bullet.Id);
        }
    }
}
