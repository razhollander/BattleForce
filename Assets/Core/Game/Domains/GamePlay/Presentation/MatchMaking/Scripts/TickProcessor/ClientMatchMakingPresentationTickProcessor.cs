using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor
{
    public class ClientMatchMakingPresentationTickProcessor : IUpdatable, IClientMatchMakingPresentationTickProcessor
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IMatchMakingPlayerControllers _playerControllers;
        private readonly IMatchMakingBulletControllers _bulletControllers;
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IWorldCameraController _worldCameraController;
        private readonly HandleBulletSpawnNetEventsCommand _handleBulletSpawnNetEventsCommand;
        private readonly HandleBulletDestroyedNetEventsCommand _handleBulletDestroyedNetEventsCommand;
        private readonly HandlePlayerSwitchTeamNetEventsCommand _handlePlayerSwitchTeamNetEventsCommand;
        private readonly HandleMatchMakingPlayerLockOnTargetsChangedNetEventsCommand _handleMatchMakingPlayerLockOnTargetsChangedNetEventsCommand;
        private readonly UpdateMatchMakingLockOnWallEffectsCommand _updateMatchMakingLockOnWallEffectsCommand;

        public ClientMatchMakingPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IMatchMakingPlayerControllers playerControllers, ICommandFactory commandFactory,
            IMatchMakingBulletControllers bulletControllers, IFullTickPacketsHandler fullTickPacketsHandler)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
            _bulletControllers = bulletControllers;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _handleBulletSpawnNetEventsCommand = commandFactory.CreateCommandVoid<HandleBulletSpawnNetEventsCommand>();
            _handleBulletDestroyedNetEventsCommand = commandFactory.CreateCommandVoid<HandleBulletDestroyedNetEventsCommand>();
            _handlePlayerSwitchTeamNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerSwitchTeamNetEventsCommand>();
            _handleMatchMakingPlayerLockOnTargetsChangedNetEventsCommand = commandFactory.CreateCommandVoid<HandleMatchMakingPlayerLockOnTargetsChangedNetEventsCommand>();
            _updateMatchMakingLockOnWallEffectsCommand = commandFactory.CreateCommandVoid<UpdateMatchMakingLockOnWallEffectsCommand>();
        }
        
        public void StartTick()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
        }
        
        public void StopTick()
        {
            _updateSubscriptionService.UnregisterUpdatable(this);
        }

        public void ManagedUpdate()
        {
            _handleBulletSpawnNetEventsCommand.Execute();
            _handleBulletDestroyedNetEventsCommand.Execute();
            _handlePlayerSwitchTeamNetEventsCommand.Execute();
            _handleMatchMakingPlayerLockOnTargetsChangedNetEventsCommand.Execute();

            _playerControllers.UpdatePlayersTransform();
            _playerControllers.UpdatePlayersBulletCooldowns();
            _bulletControllers.UpdateBulletsTransform();
            _updateMatchMakingLockOnWallEffectsCommand.Execute();
            _fullTickPacketsHandler.ClearUnprocessedPacketsByView();
        }
    }
}