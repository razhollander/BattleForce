using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateScoreGatesTransformCommand : BaseCommand, ICommandVoid
    {
        private IScoreGatesControllers _scoreGatesControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _scoreGatesControllers = _diContainer.Resolve<IScoreGatesControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var scoreGateModel in _matchDataService.ScoreGates)
            {
                _scoreGatesControllers.InterpolateScoreGateTransform(scoreGateModel.Id, scoreGateModel.Position, scoreGateModel.Rotation.ToQuaternion());
            }
        }
    }
}
