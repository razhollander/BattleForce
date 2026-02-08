using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleTeamLostNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ITeamsBoardUIController _teamsBoardUIController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.TeamLostNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var evt in events)
            {
                foreach (var kvp in evt.TotalGemsPerTeam)
                {
                    _teamsBoardUIController.UpdateTeamGems(kvp.Key, kvp.Value);
                }
            }

            events.Clear();
        }
    }
}
