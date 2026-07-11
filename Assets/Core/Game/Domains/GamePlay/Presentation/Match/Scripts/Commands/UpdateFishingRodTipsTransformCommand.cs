using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
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
                _tipControllers.InterpolateFishingRodTipTransform(tipModel.Id, tipModel.Position, rotation, casterPosition);

                // The throw-aim arrow sits on the caught enemy (the tip position while caught) and points along the throw direction.
                _secondCastEffectController.SetArrow(tipModel.Id, tipModel.Position, tipModel.EnemyCaughtArrowDirection);
            }
        }
    }
}
