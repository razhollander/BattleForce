using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateFishingRodTipsTransformCommand : BaseCommand, ICommandVoid
    {
        private IFishingRodTipControllers _tipControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _tipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
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
            }
        }
    }
}
