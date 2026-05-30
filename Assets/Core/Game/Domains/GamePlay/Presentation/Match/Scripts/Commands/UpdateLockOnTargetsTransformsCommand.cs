using Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts;
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
                foreach (var targetedEnemyId in playerModel.Spaceship.TargetedEnemyIds.AsSpan())
                {
                    var casterPlayerHeadPosition = _playerControllers.GetPlayerHeadTransform(playerModel.PlayerId).position;
                    var targetPlayerHeartPosition = _playerControllers.GetPlayerHeartTransform(targetedEnemyId).position;
                    _lockOnTargetEffectController.UpdateTargetsPositionOnPlayer(playerModel.PlayerId, targetedEnemyId, casterPlayerHeadPosition.ToVector2XY(), targetPlayerHeartPosition.ToVector2XY());
                }
            }
        }
    }
}