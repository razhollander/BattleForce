using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandlePlayerSwitchTeamNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchMakingPlayerControllers _playerControllers;
        private IMatchMakingEnvironmentWallsControllers _environmentWallsControllers;
        private SharedGamePlayConfig _sharedGamePlayConfig;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchMakingEnvironmentWallsControllers>();
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
                if (_sharedGamePlayConfig.ColorPerTeamId.TryGetValue(netEvent.TeamId, out var color))
                {
                    _playerControllers.UpdatePlayerColor(netEvent.PlayerId, color);
                    _environmentWallsControllers.AnimateWall(netEvent.TeamId);
                }
            }

            events.Clear();
        }
    }
}
