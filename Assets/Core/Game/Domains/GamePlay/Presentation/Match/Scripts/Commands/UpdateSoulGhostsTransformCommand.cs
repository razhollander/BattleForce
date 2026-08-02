using Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateSoulGhostsTransformCommand : BaseCommand, ICommandVoid
    {
        private ISoulGhostControllers _soulGhostControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _soulGhostControllers = _diContainer.Resolve<ISoulGhostControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var ghostModel in _matchDataService.SoulGhosts)
            {
                var rotation = ghostModel.Direction.ToQuaternion();
                _soulGhostControllers.InterpolateSoulGhostTransform(ghostModel.Id, ghostModel.Position, rotation);
            }
        }
    }
}
