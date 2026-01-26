using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandlePlayerSwitchTeamNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchMakingPlayerControllers _playerControllers;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private IMatchMakingEnvironmentTeamFloorControllers _environmentTeamFloorControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _environmentTeamFloorControllers = _diContainer.Resolve<IMatchMakingEnvironmentTeamFloorControllers>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
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
                var playerNewColor = _sharedGamePlayConfig.ColorPerTeamId[netEvent.TeamId];
                _playerControllers.UpdatePlayerColor(netEvent.PlayerId, playerNewColor);
                _environmentTeamFloorControllers.AnimateFloorBounce(netEvent.TeamId);
            }

            events.Clear();
        }
    }
}
