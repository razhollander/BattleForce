using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor
{
    public class ClientMatchPresentationTickProcessor : IUpdatable, IClientMatchPresentationTickProcessor
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IMatchPlayerControllers _playerControllers;
        private readonly IMatchBulletControllers _bulletControllers;
        private readonly IPowerUpBallControllers _powerUpBallControllers;
        private readonly IMatchDataService _matchDataService;
        private readonly IWorldCameraController _worldCameraController;
        private readonly HandleBulletSpawnNetEventsCommand _handleBulletSpawnNetEventsCommand;
        private readonly HandlePlayerTakeDamangeNetEventsCommand _handlePlayerTakeDamangeNetEventsCommand;
        private readonly HandleBulletDestroyedNetEventsCommand _handleBulletDestroyedNetEventsCommand;
        private readonly HandlePlayerSwapNetEventsCommand _handlePlayerSwapNetEventsCommand;
        private readonly HandleTalentCardObtainedNetEventsCommand _handleTalentCardObtainedNetEventsCommand;
        private readonly HandleTalentCardHitNetEventsCommand _handleTalentCardHitNetEventsCommand;
        private readonly HandlePowerUpBallSpawnedNetEventsCommand _handlePowerUpBallSpawneddNetEventsCommand;
        private readonly HandlePowerUpBallObtainedNetEventsCommand _handlePowerUpBallObtainedNetEventsCommand;

        public ClientMatchPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IMatchPlayerControllers playerControllers, ICommandFactory commandFactory,
            IMatchBulletControllers bulletControllers, IPowerUpBallControllers powerUpBallControllers, IMatchDataService matchDataService)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
            _bulletControllers = bulletControllers;
            _powerUpBallControllers = powerUpBallControllers;
            _matchDataService = matchDataService;
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