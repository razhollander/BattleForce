using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleTeamLostNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
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
                // Update Gems on Board
                foreach (var kvp in evt.GemsPerTeam)
                {
                    _teamsBoardUIController.UpdateTeamGems(kvp.Key, kvp.Value);
                }

                // Show Animations
                foreach (var kvp in evt.GemsGainedPerTeam)
                {
                    var teamId = kvp.Key;
                    var amount = kvp.Value;

                    foreach (var player in _matchDataService.Players)
                    {
                        if (player.TeamId == teamId && player.Spaceship.IsAlive)
                        {
                            _matchPlayerControllers.ShowGemGain(player.PlayerId, amount);
                        }
                    }
                }
            }

            events.Clear();
        }
    }
}
