using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.LocalEvents;
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
        private readonly Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc.IMatchChickenEggsControllers _chickenEggsControllers;
        private readonly IPowerUpBallControllers _powerUpBallControllers;
        private readonly IMatchPlayerUIControllers _matchPlayerUIControllers;
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IWorldCameraController _worldCameraController;
        private readonly HandleBulletSpawnNetEventsCommand _handleBulletSpawnNetEventsCommand;
        private readonly HandlePlayerTakeDamangeNetEventsCommand _handlePlayerTakeDamangeNetEventsCommand;
        private readonly HandlePlayerDiedNetEventsCommand _handlePlayerDiedNetEventsCommand;
        private readonly HandleBulletDestroyedNetEventsCommand _handleBulletDestroyedNetEventsCommand;
        private readonly HandlePlayerSwapNetEventsCommand _handlePlayerSwapNetEventsCommand;
        private readonly HandleTalentCardObtainedNetEventsCommand _handleTalentCardObtainedNetEventsCommand;
        private readonly HandleTalentCardHitNetEventsCommand _handleTalentCardHitNetEventsCommand;
        private readonly HandlePlayerSpinnedStartedNetEventsCommand _handlePlayerSpinnedStartedNetEventsCommand;
        private readonly HandlePlayerSpinnedEndedNetEventsCommand _handlePlayerSpinnedEndedNetEventsCommand;
        private readonly HandlePowerUpBallSpawnedNetEventsCommand _handlePowerUpBallSpawneddNetEventsCommand;
        private readonly HandlePowerUpBallObtainedNetEventsCommand _handlePowerUpBallObtainedNetEventsCommand;
        private readonly HandleStageEndNetEventsCommand _handleStageEndNetEventsCommand;
        private readonly HandleTeamLostNetEventsCommand _handleTeamLostNetEventsCommand;
        private readonly HandleTalentSwitchNetEventsCommand _handleTalentSwitchNetEventsCommand;
        private readonly HandleGainBoltsNetEventCommand _handleGainBoltsNetEventCommand;
        private readonly HandleEnvironmentSpringPlayerCollisionNetEventsCommand _handleEnvironmentSpringPlayerCollisionNetEventsCommand;
        private readonly HandlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand _handlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand;
        private readonly UpdateObjectTransformInsideRotatingWheelsCommand _updateObjectTransformInsideRotatingWheelsCommand;
        private readonly HandlePreparationPhaseEndedNetEventsCommand _handlePreparationPhaseEndedNetEventsCommand;
        private readonly HandleDeactivateSwapTalentNetEventsCommand _handleDeactivateSwapTalentNetEventsCommand;
        private readonly UpdateSwapFieldsTransformCommand _updateSwapFieldsTransformCommand;
        private readonly HandleSwapFieldCreatedNetEventsCommand _handleSwapFieldCreatedNetEventsCommand;
        private readonly HandleKOProjectileCreatedNetEventsCommand _handleKOProjectileCreatedNetEventsCommand;
        private readonly HandleDeactivateKOTalentNetEventsCommand _handleDeactivateKOTalentNetEventsCommand;
        private readonly HandleCreateGrapplingHookProjecitleNetEventsCommand _handleCreateGrapplingHookProjecitleNetEventsCommand;
        private readonly HandleGrapplingHookHitWallNetEventsCommand _handleGrapplingHookHitWallNetEventsCommand;
        private readonly HandleDeactivateGrapplingHookTalentNetEventsCommand _handleDeactivateGrapplingHookTalentNetEventsCommand;
        private readonly UpdateGrapplingHookProjectilesTransformCommand _updateGrapplingHookProjectilesTransformCommand;
        private readonly HandleActivateSentryGunTalentNetEventsCommand _handleActivateSentryGunTalentNetEventsCommand;
        private readonly HandleDeactivateSentryGunTalentNetEventsCommand _handleDeactivateSentryGunTalentNetEventsCommand;
        private readonly HandleKOProjectHitPlayerNetEventsCommand _handleKOProjectHitPlayerNetEventsCommand;
        private readonly HandleActivateUmbrellaTalentNetEventsCommand _handleActivateUmbrellaTalentNetEventsCommand;
        private readonly HandleDeactivateUmbrellaTalentNetEventsCommand _handleDeactivateUmbrellaTalentNetEventsCommand;
        private readonly HandleActivateChickenTalentNetEventsCommand _handleActivateChickenTalentNetEventsCommand;
        private readonly HandleDeactivateChickenTalentNetEventsCommand _handleDeactivateChickenTalentNetEventsCommand;
        private readonly HandleLayChickenEggNetEventsCommand _handleLayChickenEggNetEventsCommand;
        private readonly HandleChickenEggHitNetEventsCommand _handleChickenEggHitNetEventsCommand;
        private readonly HandleDestroyChickenEggNetEventsCommand _handleDestroyChickenEggNetEventsCommand;

        private readonly UpdateKOProjectilesTransformCommand _updateKOProjectilesTransformCommand;
        private readonly HandlePerformDashPulseNetEventsCommand _handlePerformDashPulseNetEventsCommand;
        private readonly HandleUpdatePlayerTalentStocksNetEventsCommand _handleUpdatePlayerTalentStocksNetEventsCommand;
        private readonly HandleProcessPlayerSelectedTalentFinishedCooldownEventsCommands _handleProcessPlayerSelectedTalentFinishedCooldownEventsCommands;

        public ClientMatchPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IMatchPlayerControllers playerControllers, ICommandFactory commandFactory,
            IMatchBulletControllers bulletControllers, Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc.IMatchChickenEggsControllers chickenEggsControllers, IPowerUpBallControllers powerUpBallControllers, IMatchPlayerUIControllers matchPlayerUIControllers, IFullTickPacketsHandler fullTickPacketsHandler)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
            _bulletControllers = bulletControllers;
            _chickenEggsControllers = chickenEggsControllers;
            _powerUpBallControllers = powerUpBallControllers;
            _matchPlayerUIControllers = matchPlayerUIControllers;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _handleBulletSpawnNetEventsCommand = commandFactory.CreateCommandVoid<HandleBulletSpawnNetEventsCommand>();
            _handlePlayerTakeDamangeNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerTakeDamangeNetEventsCommand>();
            _handlePlayerDiedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerDiedNetEventsCommand>();
            _handleBulletDestroyedNetEventsCommand = commandFactory.CreateCommandVoid<HandleBulletDestroyedNetEventsCommand>();
            _handlePlayerSwapNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerSwapNetEventsCommand>();
            _handleTalentCardObtainedNetEventsCommand = commandFactory.CreateCommandVoid<HandleTalentCardObtainedNetEventsCommand>();
            _handleTalentCardHitNetEventsCommand = commandFactory.CreateCommandVoid<HandleTalentCardHitNetEventsCommand>();
            _handlePlayerSpinnedStartedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerSpinnedStartedNetEventsCommand>();
            _handlePlayerSpinnedEndedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerSpinnedEndedNetEventsCommand>();
            _handlePowerUpBallSpawneddNetEventsCommand = commandFactory.CreateCommandVoid<HandlePowerUpBallSpawnedNetEventsCommand>();
            _handlePowerUpBallObtainedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePowerUpBallObtainedNetEventsCommand>();
            _handleStageEndNetEventsCommand = commandFactory.CreateCommandVoid<HandleStageEndNetEventsCommand>();
            _handleTeamLostNetEventsCommand = commandFactory.CreateCommandVoid<HandleTeamLostNetEventsCommand>();
            _handleTalentSwitchNetEventsCommand = commandFactory.CreateCommandVoid<HandleTalentSwitchNetEventsCommand>();
            _handleGainBoltsNetEventCommand = commandFactory.CreateCommandVoid<HandleGainBoltsNetEventCommand>();
            _handleEnvironmentSpringPlayerCollisionNetEventsCommand = commandFactory.CreateCommandVoid<HandleEnvironmentSpringPlayerCollisionNetEventsCommand>();
            _handlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand>();
            _updateObjectTransformInsideRotatingWheelsCommand = commandFactory.CreateCommandVoid<UpdateObjectTransformInsideRotatingWheelsCommand>();
            _handlePreparationPhaseEndedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePreparationPhaseEndedNetEventsCommand>();
            _handleDeactivateSwapTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateSwapTalentNetEventsCommand>();
            _updateSwapFieldsTransformCommand = commandFactory.CreateCommandVoid<UpdateSwapFieldsTransformCommand>();
            _handleSwapFieldCreatedNetEventsCommand = commandFactory.CreateCommandVoid<HandleSwapFieldCreatedNetEventsCommand>();
            _handleKOProjectileCreatedNetEventsCommand = commandFactory.CreateCommandVoid<HandleKOProjectileCreatedNetEventsCommand>();
            _handleDeactivateKOTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateKOTalentNetEventsCommand>();
            _handleCreateGrapplingHookProjecitleNetEventsCommand = commandFactory.CreateCommandVoid<HandleCreateGrapplingHookProjecitleNetEventsCommand>();
            _handleGrapplingHookHitWallNetEventsCommand = commandFactory.CreateCommandVoid<HandleGrapplingHookHitWallNetEventsCommand>();
            _handleDeactivateGrapplingHookTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateGrapplingHookTalentNetEventsCommand>();
            _updateGrapplingHookProjectilesTransformCommand = commandFactory.CreateCommandVoid<UpdateGrapplingHookProjectilesTransformCommand>();
            _handleActivateSentryGunTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleActivateSentryGunTalentNetEventsCommand>();
            _handleDeactivateSentryGunTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateSentryGunTalentNetEventsCommand>();
            _handleActivateUmbrellaTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleActivateUmbrellaTalentNetEventsCommand>();
            _handleDeactivateUmbrellaTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateUmbrellaTalentNetEventsCommand>();
            _handleActivateChickenTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleActivateChickenTalentNetEventsCommand>();
            _handleDeactivateChickenTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateChickenTalentNetEventsCommand>();
            _handleLayChickenEggNetEventsCommand = commandFactory.CreateCommandVoid<HandleLayChickenEggNetEventsCommand>();
            _handleChickenEggHitNetEventsCommand = commandFactory.CreateCommandVoid<HandleChickenEggHitNetEventsCommand>();
            _handleDestroyChickenEggNetEventsCommand = commandFactory.CreateCommandVoid<HandleDestroyChickenEggNetEventsCommand>();

            _handleKOProjectHitPlayerNetEventsCommand = commandFactory.CreateCommandVoid<HandleKOProjectHitPlayerNetEventsCommand>();
            _updateKOProjectilesTransformCommand = commandFactory.CreateCommandVoid<UpdateKOProjectilesTransformCommand>();
            _handlePerformDashPulseNetEventsCommand = commandFactory.CreateCommandVoid<HandlePerformDashPulseNetEventsCommand>();
            _handleUpdatePlayerTalentStocksNetEventsCommand = commandFactory.CreateCommandVoid<HandleUpdatePlayerTalentStocksNetEventsCommand>();
            _handleProcessPlayerSelectedTalentFinishedCooldownEventsCommands = commandFactory.CreateCommandVoid<HandleProcessPlayerSelectedTalentFinishedCooldownEventsCommands>();
        }
        
        public void InitEntryPoint()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
        }
        
        public void InitExitPoint()
        {
            _updateSubscriptionService.UnregisterUpdatable(this);
        }

        public void ManagedUpdate()
        {
            var lastProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer;

            _handleBulletSpawnNetEventsCommand.Execute();
            _handleBulletDestroyedNetEventsCommand.Execute();
            _handlePlayerTakeDamangeNetEventsCommand.Execute();
            _handlePlayerDiedNetEventsCommand.Execute();
            _handlePlayerSwapNetEventsCommand.Execute();
            _handleTalentCardHitNetEventsCommand.Execute();
            _handlePlayerSpinnedStartedNetEventsCommand.Execute();
            _handlePlayerSpinnedEndedNetEventsCommand.Execute();
            _handlePowerUpBallSpawneddNetEventsCommand.Execute();
            _handlePowerUpBallObtainedNetEventsCommand.Execute();
            _handleStageEndNetEventsCommand.Execute();
            _handleTeamLostNetEventsCommand.Execute();
            _handleTalentSwitchNetEventsCommand.Execute();
            _handleGainBoltsNetEventCommand.Execute();
            _handleEnvironmentSpringPlayerCollisionNetEventsCommand.Execute();
            _handlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand.Execute();
            _handlePreparationPhaseEndedNetEventsCommand.Execute();
            _matchPlayerUIControllers.UpdatePlayersTalentCooldowns(lastProcessedTickFromServer);
            _playerControllers.UpdatePlayersTalentCooldowns(lastProcessedTickFromServer);
            _playerControllers.UpdatePlayersTickDeltas();
            _handleSwapFieldCreatedNetEventsCommand.SetTick(lastProcessedTickFromServer).Execute();
            _handleDeactivateSwapTalentNetEventsCommand.Execute();
            _updateSwapFieldsTransformCommand.SetTick(lastProcessedTickFromServer).Execute();// must be after _playerControllers.UpdatePlayersTransform();
            _handleKOProjectileCreatedNetEventsCommand.Execute(); // must be after _playerControllers.UpdatePlayersTransform();
            _handleKOProjectHitPlayerNetEventsCommand.Execute();
            _handleDeactivateKOTalentNetEventsCommand.Execute();
            _handleCreateGrapplingHookProjecitleNetEventsCommand.Execute();
            _handleGrapplingHookHitWallNetEventsCommand.Execute();
            _handleDeactivateGrapplingHookTalentNetEventsCommand.Execute();
            _handleActivateSentryGunTalentNetEventsCommand.Execute();
            _handleDeactivateSentryGunTalentNetEventsCommand.Execute();
            _handleActivateUmbrellaTalentNetEventsCommand.Execute();
            _handleDeactivateUmbrellaTalentNetEventsCommand.Execute();
            _handleActivateChickenTalentNetEventsCommand.Execute();
            _handleDeactivateChickenTalentNetEventsCommand.Execute();
            _handleLayChickenEggNetEventsCommand.Execute();
            _handleChickenEggHitNetEventsCommand.Execute();
            _handleDestroyChickenEggNetEventsCommand.Execute();

            _handlePerformDashPulseNetEventsCommand.Execute();
            _handleUpdatePlayerTalentStocksNetEventsCommand.Execute();
            _updateKOProjectilesTransformCommand.Execute(); // must be after _handleDeactivateKOTalentNetEventsCommand.Execute();
            _updateGrapplingHookProjectilesTransformCommand.Execute();
            _playerControllers.UpdatePlayersBulletCooldowns();
            _bulletControllers.UpdateBulletsTransform();
            _chickenEggsControllers.UpdateEggsTransform();
            _powerUpBallControllers.UpdatePowerUpBallsTransform();
            _updateObjectTransformInsideRotatingWheelsCommand.Execute();
            _handleProcessPlayerSelectedTalentFinishedCooldownEventsCommands.Execute();
            _handleTalentCardObtainedNetEventsCommand.SetCurrentServerTick(lastProcessedTickFromServer).Execute();
        }
    }
}