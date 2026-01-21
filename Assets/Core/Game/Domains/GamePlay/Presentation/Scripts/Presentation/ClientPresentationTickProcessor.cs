using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation
{
    public class ClientPresentationTickProcessor : IUpdatable, IClientPresentationTickProcessor
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IPlayerControllers _playerControllers;
        private readonly IBulletControllers _bulletControllers;
        private readonly IPowerUpBallControllers _powerUpBallControllers;
        private readonly HandleBulletSpawnNetEventsCommand _handleBulletSpawnNetEventsCommand;
        private readonly HandlePlayerTakeDamangeNetEventsCommand _handlePlayerTakeDamangeNetEventsCommand;
        private readonly HandleBulletDestroyedNetEventsCommand _handleBulletDestroyedNetEventsCommand;
        private readonly HandlePlayerSwapNetEventsCommand _handlePlayerSwapNetEventsCommand;
        private readonly HandleTalentCardObtainedNetEventsCommand _handleTalentCardObtainedNetEventsCommand;
        private readonly HandleTalentCardHitNetEventsCommand _handleTalentCardHitNetEventsCommand;
        private readonly HandlePowerUpBallSpawnedNetEventsCommand _handlePowerUpBallSpawneddNetEventsCommand;
        private readonly HandlePowerUpBallObtainedNetEventsCommand _handlePowerUpBallObtainedNetEventsCommand;

        public ClientPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IPlayerControllers playerControllers, ICommandFactory commandFactory,
            IBulletControllers bulletControllers, IPowerUpBallControllers powerUpBallControllers)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
            _bulletControllers = bulletControllers;
            _powerUpBallControllers = powerUpBallControllers;
            _handleBulletSpawnNetEventsCommand = commandFactory.CreateCommandVoid<HandleBulletSpawnNetEventsCommand>();
            _handlePlayerTakeDamangeNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerTakeDamangeNetEventsCommand>();
            _handleBulletDestroyedNetEventsCommand = commandFactory.CreateCommandVoid<HandleBulletDestroyedNetEventsCommand>();
            _handlePlayerSwapNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerSwapNetEventsCommand>();
            _handleTalentCardObtainedNetEventsCommand = commandFactory.CreateCommandVoid<HandleTalentCardObtainedNetEventsCommand>();
            _handleTalentCardHitNetEventsCommand = commandFactory.CreateCommandVoid<HandleTalentCardHitNetEventsCommand>();
            _handlePowerUpBallSpawneddNetEventsCommand = commandFactory.CreateCommandVoid<HandlePowerUpBallSpawnedNetEventsCommand>();
            _handlePowerUpBallObtainedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePowerUpBallObtainedNetEventsCommand>();
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
            _handlePlayerTakeDamangeNetEventsCommand.Execute();
            _handlePlayerSwapNetEventsCommand.Execute();
            _handleTalentCardHitNetEventsCommand.Execute();
            _handlePowerUpBallSpawneddNetEventsCommand.Execute();
            _handlePowerUpBallObtainedNetEventsCommand.Execute();

            _playerControllers.UpdatePlayersTransform();
            _playerControllers.UpdatePlayersBulletCooldowns();
            _bulletControllers.UpdateBulletsTransform();
            _powerUpBallControllers.UpdatePowerUpBallsTransform();
            
            _handleTalentCardObtainedNetEventsCommand.Execute();
        }
    }
}