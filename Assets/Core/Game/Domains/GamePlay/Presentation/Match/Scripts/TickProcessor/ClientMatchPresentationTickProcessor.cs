using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.LocalEvents;
using Core.Scripts.Mvc.WorldCamera;
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
        private readonly HandleEnvironmentSpikePlayerCollisionNetEventsCommand _handleEnvironmentSpikePlayerCollisionNetEventsCommand;
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
        private readonly HandleLayChickenEggNetEventsCommand _handleLayChickenEggNetEventsCommand;
        private readonly HandleChickenEggHitNetEventsCommand _handleChickenEggHitNetEventsCommand;
        private readonly UpdateKOProjectilesTransformCommand _updateKOProjectilesTransformCommand;
        private readonly HandlePerformDashPulseNetEventsCommand _handlePerformDashPulseNetEventsCommand;
        private readonly HandleUpdatePlayerTalentStocksNetEventsCommand _handleUpdatePlayerTalentStocksNetEventsCommand;
        private readonly HandleProcessPlayerSelectedTalentFinishedCooldownEventsCommands _handleProcessPlayerSelectedTalentFinishedCooldownEventsCommands;
        private readonly HandleCreateMagenticPullFieldNetEventsCommand _handleCreateMagenticPullFieldNetEventsCommand;
        private readonly HandleActivateYearsOfPainTalentNetEventsCommand _handleActivateYearsOfPainTalentNetEventsCommand;
        private readonly HandlePlayerLockOnTargetsChangedNetEventsCommand _handlePlayerLockOnTargetsChangedNetEventsCommand;
        private readonly HandlePlayerLockedOnTargetHitNetEventsCommand _handlePlayerLockedOnTargetHitNetEventsCommand;
        private readonly HandlePlayerPowerUpChangedNetEventsCommand _handlePlayerPowerUpChangedNetEventsCommand;
        private readonly HandleSonicSlapActivatedNetEventsCommand _handleSonicSlapActivatedNetEventsCommand;
        private readonly HandlePerformGalacticPullNetEventsCommand _handlePerformGalacticPullNetEventsCommand;
        private readonly HandleDeactivateGalacticForceFieldNetEventsCommand _handleDeactivateGalacticForceFieldNetEventsCommand;
        private readonly HandleActivateNukePowerUpNetEventsCommand _handleActivateNukePowerUpNetEventsCommand;
        private readonly HandleDeactivateShufflePowerUpNetEventsCommand _handleDeactivateShufflePowerUpNetEventsCommand;
        private readonly HandleShuffleSwapPlayerPositionNetEventsCommand _handleShuffleSwapPlayerPositionNetEventsCommand;
        private readonly HandleActivateShuffleNetEventsCommand _handleActivateShuffleNetEventsCommand;
        private readonly HandleStartPowerUpGrantingPhaseNetEventsCommand _handleStartPowerUpGrantingPhaseNetEventsCommand;
        private readonly HandleEndPowerUpGrantingPhaseNetEventsCommand _handleEndPowerUpGrantingPhaseNetEventsCommand;
        private readonly UpdateLockOnTargetsTransformsCommand _updateLockOnTargetsTransformsCommand;

        public ClientMatchPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IMatchPlayerControllers playerControllers, ICommandFactory commandFactory,
            IMatchBulletControllers bulletControllers, IPowerUpBallControllers powerUpBallControllers, IMatchPlayerUIControllers matchPlayerUIControllers, IFullTickPacketsHandler fullTickPacketsHandler)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
            _bulletControllers = bulletControllers;
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
            _handleEnvironmentSpikePlayerCollisionNetEventsCommand = commandFactory.CreateCommandVoid<Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents.HandleEnvironmentSpikePlayerCollisionNetEventsCommand>();
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
            _handleLayChickenEggNetEventsCommand = commandFactory.CreateCommandVoid<HandleLayChickenEggNetEventsCommand>();
            _handleChickenEggHitNetEventsCommand = commandFactory.CreateCommandVoid<HandleChickenEggHitNetEventsCommand>();
            _handleKOProjectHitPlayerNetEventsCommand = commandFactory.CreateCommandVoid<HandleKOProjectHitPlayerNetEventsCommand>();
            _updateKOProjectilesTransformCommand = commandFactory.CreateCommandVoid<UpdateKOProjectilesTransformCommand>();
            _handlePerformDashPulseNetEventsCommand = commandFactory.CreateCommandVoid<HandlePerformDashPulseNetEventsCommand>();
            _handleUpdatePlayerTalentStocksNetEventsCommand = commandFactory.CreateCommandVoid<HandleUpdatePlayerTalentStocksNetEventsCommand>();
            _handleCreateMagenticPullFieldNetEventsCommand = commandFactory.CreateCommandVoid<HandleCreateMagenticPullFieldNetEventsCommand>();
            _handleProcessPlayerSelectedTalentFinishedCooldownEventsCommands = commandFactory.CreateCommandVoid<HandleProcessPlayerSelectedTalentFinishedCooldownEventsCommands>();
            _handleActivateYearsOfPainTalentNetEventsCommand = commandFactory.CreateCommandVoid<HandleActivateYearsOfPainTalentNetEventsCommand>();
            _handlePlayerLockOnTargetsChangedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerLockOnTargetsChangedNetEventsCommand>();
            _handlePlayerLockedOnTargetHitNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerLockedOnTargetHitNetEventsCommand>();
            _handlePlayerPowerUpChangedNetEventsCommand = commandFactory.CreateCommandVoid<HandlePlayerPowerUpChangedNetEventsCommand>();
            _handleSonicSlapActivatedNetEventsCommand = commandFactory.CreateCommandVoid<HandleSonicSlapActivatedNetEventsCommand>();
            _handlePerformGalacticPullNetEventsCommand = commandFactory.CreateCommandVoid<HandlePerformGalacticPullNetEventsCommand>();
            _handleDeactivateGalacticForceFieldNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateGalacticForceFieldNetEventsCommand>();
            _handleActivateNukePowerUpNetEventsCommand = commandFactory.CreateCommandVoid<HandleActivateNukePowerUpNetEventsCommand>();
            _handleDeactivateShufflePowerUpNetEventsCommand = commandFactory.CreateCommandVoid<HandleDeactivateShufflePowerUpNetEventsCommand>();
            _handleShuffleSwapPlayerPositionNetEventsCommand = commandFactory.CreateCommandVoid<HandleShuffleSwapPlayerPositionNetEventsCommand>();
            _handleActivateShuffleNetEventsCommand = commandFactory.CreateCommandVoid<HandleActivateShuffleNetEventsCommand>();
            _handleStartPowerUpGrantingPhaseNetEventsCommand = commandFactory.CreateCommandVoid<HandleStartPowerUpGrantingPhaseNetEventsCommand>();
            _handleEndPowerUpGrantingPhaseNetEventsCommand = commandFactory.CreateCommandVoid<HandleEndPowerUpGrantingPhaseNetEventsCommand>();
            _updateLockOnTargetsTransformsCommand = commandFactory.CreateCommandVoid<UpdateLockOnTargetsTransformsCommand>();
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
            _handleEnvironmentSpikePlayerCollisionNetEventsCommand.Execute();
            _handlePlayerToEnvironmentTeleportGateCollisionNetEventsCommand.Execute();
            _handlePreparationPhaseEndedNetEventsCommand.Execute();
            _matchPlayerUIControllers.UpdatePlayersTalentCooldowns(lastProcessedTickFromServer);
            _playerControllers.UpdatePlayersTalentCooldowns(lastProcessedTickFromServer);
            _playerControllers.UpdatePlayersTickDeltas();
            _handleSwapFieldCreatedNetEventsCommand.SetTick(lastProcessedTickFromServer).Execute();
            _handleDeactivateSwapTalentNetEventsCommand.Execute();
            _updateSwapFieldsTransformCommand.SetTick(lastProcessedTickFromServer).Execute();// must be after _playerControllers.UpdatePlayersTickDeltas();
            _handleKOProjectileCreatedNetEventsCommand.Execute(); // must be after _playerControllers.UpdatePlayersTickDeltas();
            _handleKOProjectHitPlayerNetEventsCommand.Execute();
            _handleDeactivateKOTalentNetEventsCommand.Execute();
            _handleCreateGrapplingHookProjecitleNetEventsCommand.Execute();
            _handleGrapplingHookHitWallNetEventsCommand.Execute();
            _handleDeactivateGrapplingHookTalentNetEventsCommand.Execute();
            _handleActivateSentryGunTalentNetEventsCommand.Execute();
            _handleDeactivateSentryGunTalentNetEventsCommand.Execute();
            _handleActivateUmbrellaTalentNetEventsCommand.Execute();
            _handleDeactivateUmbrellaTalentNetEventsCommand.Execute();
            _handleLayChickenEggNetEventsCommand.Execute(); // must be after _handleTalentSwitchNetEventsCommand.Execute();
            _handleChickenEggHitNetEventsCommand.Execute();
            _handlePerformDashPulseNetEventsCommand.Execute();
            _handleUpdatePlayerTalentStocksNetEventsCommand.Execute();
            _handleCreateMagenticPullFieldNetEventsCommand.Execute();
            _handleActivateYearsOfPainTalentNetEventsCommand.Execute();
            _updateKOProjectilesTransformCommand.Execute(); // must be after _handleDeactivateKOTalentNetEventsCommand.Execute();
            _updateGrapplingHookProjectilesTransformCommand.Execute();
            _playerControllers.UpdatePlayersBulletCooldowns();
            _bulletControllers.UpdateBulletsTransform();
            _powerUpBallControllers.UpdatePowerUpBallsTransform();
            _updateObjectTransformInsideRotatingWheelsCommand.Execute();
            _handleProcessPlayerSelectedTalentFinishedCooldownEventsCommands.Execute();
            _handlePlayerLockOnTargetsChangedNetEventsCommand.Execute();
            _handlePlayerLockedOnTargetHitNetEventsCommand.Execute();
            _handlePlayerPowerUpChangedNetEventsCommand.Execute();
            _handleSonicSlapActivatedNetEventsCommand.Execute();
            _handlePerformGalacticPullNetEventsCommand.Execute();
            _handleDeactivateGalacticForceFieldNetEventsCommand.Execute();
            _handleActivateNukePowerUpNetEventsCommand.Execute();
            _handleDeactivateShufflePowerUpNetEventsCommand.Execute();
            _handleShuffleSwapPlayerPositionNetEventsCommand.Execute();
            _handleActivateShuffleNetEventsCommand.Execute();
            _handleStartPowerUpGrantingPhaseNetEventsCommand.Execute();
            _handleEndPowerUpGrantingPhaseNetEventsCommand.Execute();
            _updateLockOnTargetsTransformsCommand.Execute(); // must be after _handlePlayerLockOnTargetsChangedNetEventsCommand.Execute() & _playerControllers.UpdatePlayersTickDeltas();
            _fullTickPacketsHandler.ClearUnprocessedPacketsByView();
        }
    }
}