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

        public ClientPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IPlayerControllers playerControllers, ICommandFactory commandFactory, IBulletControllers bulletControllers)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
            _bulletControllers = bulletControllers;
            _commandFactory = commandFactory;
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
            _commandFactory.CreateCommandVoid<HandleBulletSpawnNetEventsCommand>().Execute();
            _commandFactory.CreateCommandVoid<HandlePlayerTakeDamangeNetEventsCommand>().Execute();
            _commandFactory.CreateCommandVoid<HandleBulletDestroyedNetEventsCommand>().Execute();
            _playerControllers.UpdatePlayersTransform();
            _playerControllers.UpdatePlayersBulletCooldowns();
            _bulletControllers.UpdateBulletsTransform();
        }
    }
}