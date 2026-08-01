using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateFishingRodTipsTransformCommand : BaseCommand, ICommandVoid
    {
        private IFishingRodTipControllers _tipControllers;
        private ISecondCastAimArrowControllers _secondCastAimArrowControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _tipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _secondCastAimArrowControllers = _diContainer.Resolve<ISecondCastAimArrowControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var tipModel in _matchDataService.FishingRodTips)
            {
                var casterPosition = _playerControllers.GetPlayerPosition(tipModel.CasterPlayerId);
                var directionFromCaster = tipModel.Position - casterPosition;
                var rotation = directionFromCaster.ToQuaternion();
                var shouldRodLookRight = directionFromCaster.x > 0;
                _playerControllers.SetPlayerFishingRodStickDirection(tipModel.CasterPlayerId, shouldRodLookRight);
                
                var lineStartPosition = _playerControllers.GetPlayerFishingRodTipPivotPosition(tipModel.CasterPlayerId);
                _tipControllers.InterpolateFishingRodTipTransform(tipModel.Id, tipModel.Position, rotation, lineStartPosition);

                if (tipModel.Phase == FishingRodTipPhase.CaughtEnemy)
                {
                    _secondCastAimArrowControllers.SetArrow(tipModel.Id, tipModel.Position, tipModel.EnemyCaughtArrowDirection);
                }
            }
        }
    }
}
