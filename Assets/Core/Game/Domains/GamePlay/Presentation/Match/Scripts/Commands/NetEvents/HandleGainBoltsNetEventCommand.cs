using Core.Game.Domains.GamePlay.Presentation.Match.Features.FX.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleGainBoltsNetEventCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IGainBoltFxController _gainBoltFxController;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _gainBoltFxController = _diContainer.Resolve<IGainBoltFxController>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.GainBoltsNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var evt in events)
            {
                var player = _matchDataService.GetPlayer(evt.PlayerId);
                _teamsBoardUIController.UpdateTeamBolts(player.TeamId, evt.TotalTeamBolts);

                var playerPosition = player.Spaceship.Transform.Position.ToUnityVector2();
                _gainBoltFxController.ShowFx(evt.GainedAmount, playerPosition);
            }

            events.Clear();
        }
    }
}
