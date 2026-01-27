using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandlePlayerSwitchTeamNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchMakingPlayerControllers _playerControllers;
        private IMatchMakingEnvironmentTeamFloorControllers _environmentTeamFloorControllers;
        private IMatchMakingDataService _matchMakingDataService;
        private PresentationGamePlayConfig _gameplayConfig;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _environmentTeamFloorControllers = _diContainer.Resolve<IMatchMakingEnvironmentTeamFloorControllers>();
            _gameplayConfig =_diContainer.Resolve<PresentationGamePlayConfig>();
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PlayerSwitchTeamNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                var teamId = netEvent.TeamId;
                var playerNewColor = _gameplayConfig.ColorPerTeamId[teamId];
                _playerControllers.UpdatePlayerColor(netEvent.PlayerId, playerNewColor);
                _matchMakingDataService.GetPlayer(netEvent.PlayerId).TeamId = teamId;
                _environmentTeamFloorControllers.AnimateFloorBounce(teamId);
            }

            events.Clear();
        }
    }
}
