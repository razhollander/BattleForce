using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleStageEndNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IStageEndedUiController _stageEndedUiController;
        private IWorldCameraController _worldCameraController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _stageEndedUiController = _diContainer.Resolve<IStageEndedUiController>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
        }

        public void Execute()
        {
            var stageEndEvents = _cachedPresentationEventsService.StageEndNetEvents;
            if (stageEndEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var stageEndEvent in stageEndEvents)
            {
                var winningTeamId = stageEndEvent.WinningTeamId;
                var isThereOnlyOneTeam = winningTeamId==0;

                if(!isThereOnlyOneTeam)
                {
                    _stageEndedUiController.Show(winningTeamId, stageEndEvent.JemsWonPerTeam);
                    _worldCameraController.ShakeCamera(10,0.5f);
                }
            }

            stageEndEvents.Clear();
        }
    }
}
