using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation
{
    public class ClientPresentationTickProcessor : IUpdatable, IClientPresentationTickProcessor
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IPlayerControllers _playerControllers;
        private readonly ICommandFactory _commandFactory;
        private readonly IBulletControllers _bulletControllers;
        private readonly HandleBulletSpawnNetEventsCommand _handleBulletSpawnNetEventsCommand;
        private readonly HandlePlayerTakeDamangeNetEventsCommand _handlePlayerTakeDamangeNetEventsCommand;
        private readonly HandleBulletDestroyedNetEventsCommand _handleBulletDestroyedNetEventsCommand;

        public ClientPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IPlayerControllers playerControllers, ICommandFactory commandFactory, IBulletControllers bulletControllers)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
            _bulletControllers = bulletControllers;
            _commandFactory = commandFactory;
            _handleBulletSpawnNetEventsCommand = _commandFactory.CreateCommandVoid<HandleBulletSpawnNetEventsCommand>();
            _handlePlayerTakeDamangeNetEventsCommand = _commandFactory.CreateCommandVoid<HandlePlayerTakeDamangeNetEventsCommand>();
            _handleBulletDestroyedNetEventsCommand = _commandFactory.CreateCommandVoid<HandleBulletDestroyedNetEventsCommand>();
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
            _handlePlayerTakeDamangeNetEventsCommand.Execute();
            _handleBulletDestroyedNetEventsCommand.Execute();
         
            _playerControllers.UpdatePlayersTransform();
            _playerControllers.UpdatePlayersBulletCooldowns();
            _bulletControllers.UpdateBulletsTransform();
        }
    }
}