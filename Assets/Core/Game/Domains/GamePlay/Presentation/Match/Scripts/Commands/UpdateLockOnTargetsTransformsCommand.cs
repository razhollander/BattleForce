using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateLockOnTargetsTransformsCommand : BaseCommand, ICommandVoid
    {
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var playerModel in _matchDataService.Players)
            {
                var casterPlayerHeadPosition = _playerControllers.GetPlayerHeadTransform(playerModel.PlayerId).position.ToVector2XY();
                
                foreach (var targetedEnemy in playerModel.Spaceship.TargetedEnemyIds.AsSpan())
                {
                    var targetedEnemyId = targetedEnemy.PlayerTargetId;
                    var targetPlayerHeartPosition = _playerControllers.GetPlayerHeartTransform(targetedEnemyId).position.ToVector2XY();
                    _lockOnTargetEffectController.UpdateTargetsPositionOnPlayer(playerModel.PlayerId, targetedEnemyId, casterPlayerHeadPosition, targetPlayerHeartPosition);
                }
            }
        }
    }
}