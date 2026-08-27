using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateLockOnTargetEffectsCommand : BaseCommand, ICommandVoid
    {
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        private IMatchPlayerControllers _playerControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IMoleControllers _moleControllers;
        private IMatchDataService _matchDataService;

        private int _tick;

        public UpdateLockOnTargetEffectsCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var playerModel in _matchDataService.Players)
            {
                var casterPlayerHeadPosition = _playerControllers.GetPlayerHeadTransform(playerModel.PlayerId).position.ToVector2XY();

                foreach (var targetedObject in playerModel.Spaceship.LockOnTargetObjects.AsSpan())
                {
                    _lockOnTargetEffectController.UpdateTargetRetentionProgress(playerModel.PlayerId, targetedObject, _tick);

                    if (!TryGetTargetPosition(targetedObject, out var targetPosition))
                    {
                        continue;
                    }

                    _lockOnTargetEffectController.UpdateTargetsPositionOnPlayer(playerModel.PlayerId, targetedObject.GetKey(), casterPlayerHeadPosition, targetPosition);
                }
            }
        }

        private bool TryGetTargetPosition(ObjectLockedOnTargetS2C targetedEnemy, out Vector2 targetPosition)
        {
            switch (targetedEnemy.TargetType)
            {
                case LockOnTargetType.PowerUpBall:
                    targetPosition = _powerUpBallControllers.GetPowerUpBallPosition(targetedEnemy.TargetId);
                    return true;
                case LockOnTargetType.Mole:
                    return _moleControllers.TryGetMoleHolePosition(targetedEnemy.TargetId, out targetPosition);
                default:
                    targetPosition = _playerControllers.GetPlayerHeartTransform(targetedEnemy.TargetId).position.ToVector2XY();
                    return true;
            }
        }
    }
}