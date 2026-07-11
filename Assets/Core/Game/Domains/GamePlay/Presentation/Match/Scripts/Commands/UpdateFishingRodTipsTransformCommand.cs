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
        private ISecondCastEffectController _secondCastEffectController;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _tipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _secondCastEffectController = _diContainer.Resolve<ISecondCastEffectController>();
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
                // The rod looks toward whichever side the projectile currently sits relative to the caster.
                _playerControllers.SetPlayerFishingRodStickDirection(tipModel.CasterPlayerId, directionFromCaster.x > 0);
                // The fishing line starts from the stick's tip pivot rather than the caster's centre.
                var lineStartPosition = _playerControllers.GetPlayerFishingRodTipPivotPosition(tipModel.CasterPlayerId);
                _tipControllers.InterpolateFishingRodTipTransform(tipModel.Id, tipModel.Position, rotation, lineStartPosition);

                // The throw-aim arrow is only shown while the tip holds a caught enemy; it sits on that enemy (the tip
                // position) and points along the throw direction.
                if (tipModel.Phase == FishingRodTipPhase.CaughtEnemy)
                {
                    _secondCastEffectController.SetArrow(tipModel.Id, tipModel.Position, tipModel.EnemyCaughtArrowDirection);
                }
                else
                {
                    _secondCastEffectController.RemoveArrow(tipModel.Id);
                }
            }
        }
    }
}
