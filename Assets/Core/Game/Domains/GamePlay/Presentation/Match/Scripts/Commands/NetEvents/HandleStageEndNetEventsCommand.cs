using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleStageEndNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IStageEndedUiController _stageEndedUiController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _stageEndedUiController = _diContainer.Resolve<IStageEndedUiController>();
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
                _stageEndedUiController.Show(stageEndEvent.WinningTeamId, stageEndEvent.TotalJemsPerTeam);
            }

            stageEndEvents.Clear();
        }
    }
}
