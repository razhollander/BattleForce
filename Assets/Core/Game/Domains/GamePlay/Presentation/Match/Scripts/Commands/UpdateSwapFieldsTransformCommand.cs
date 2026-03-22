using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateSwapFieldsTransformCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private ISwapFieldControllers _swapFieldControllers;
        private IMatchPlayerControllers _playerControllers;
        
        private int _tick;

        public UpdateSwapFieldsTransformCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _swapFieldControllers = _diContainer.Resolve<ISwapFieldControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            foreach (var swapFieldModel in _matchDataService.SwapFields)
            {
                var position = _playerControllers.GetPlayerPosition(swapFieldModel.PlayerCasterId);
                var swapFieldRadius = swapFieldModel.CalculateCurrentRadiusForTick(_tick);
                _swapFieldControllers.SetSwapFieldTransform(swapFieldModel.Id, position, swapFieldRadius);
            }
        }
    }
}