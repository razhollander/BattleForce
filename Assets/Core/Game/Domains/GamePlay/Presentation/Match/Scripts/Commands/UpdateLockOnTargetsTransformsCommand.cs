using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateLockOnTargetsTransformsCommand : BaseCommand, ICommandVoid
    {
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        private IMatchPlayerControllers _playerControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
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

                foreach (var targetedEnemy in playerModel.Spaceship.TargetedEnemyIds.AsSpan())
                {
                    var targetedId = targetedEnemy.PlayerTargetId;
                    var targetPosition = GetTargetPosition(targetedEnemy);
                    _lockOnTargetEffectController.UpdateTargetsPositionOnPlayer(playerModel.PlayerId, targetedId, casterPlayerHeadPosition, targetPosition);
                }
            }
        }

        private Vector2 GetTargetPosition(ObjectLockedOnTargetS2C targetedEnemy)
        {
            switch (targetedEnemy.TargetType)
            {
                case LockOnTargetType.PowerUpBall:
                    return _powerUpBallControllers.GetPowerUpBallPosition(targetedEnemy.PlayerTargetId);
                default:
                    return _playerControllers.GetPlayerHeartTransform(targetedEnemy.PlayerTargetId).position.ToVector2XY();
            }
        }
    }
}