using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateFrigidBlocksTransformCommand : BaseCommand, ICommandVoid
    {
        private IFrigidBlocksControllers _frigidBlocksControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _frigidBlocksControllers = _diContainer.Resolve<IFrigidBlocksControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var blockModel in _matchDataService.FrigidBlocks)
            {
                _frigidBlocksControllers.InterpolateFrigidBlockTransform(blockModel.Id, blockModel.Position, blockModel.Rotation.ToQuaternion());
            }
        }
    }
}
