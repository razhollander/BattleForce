using System;
using System.Diagnostics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers
{
    public class ServerMatchNetworkTickProcessor : ITickProcessor
    {
        private readonly NetworkConfig _networkConfig;
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private readonly IMatchDataService _matchDataService;
        private readonly IMatchPlayerJoinPacketsHandler _matchPlayerJoinPacketsHandler;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IStateMachineService _stateMachineService;
        private readonly ICommandFactory _commandFactory;
        private readonly ITickService _tickService;
        private readonly IHeadLessQuitterController _headLessQuitterController;
        private readonly IStageDataService _stageDataService;
        private readonly IOverrideableNetEventsService _overrideableNetEventsService;
        private readonly IClientsNetworkDataService _clientsNetworkDataService;
        private readonly IPlayersPowerUpsManager _playersPowerUpsManager;

        private TryDamagePlayersInLavaCommand _tryDamagePlayersInLavaCommand;
        private TrySpawnPowerUpBallsCommand _trySpawnPowerUpBallsCommand;
        private ApplyGalacticPullForcesCommand _applyGalacticPullForcesCommand;
        private TryDeactivateEndedGalacticFieldsCommand _tryDeactivateEndedGalacticFieldsCommand;
        private StepPhysiscsSimulationCommand _stepPhysiscsSimulationCommand;
        private StepFrigidBlocksCommand _stepFrigidBlocksCommand;
        private StepTimersCommand _stepTimersCommand;
        private TryEndPlayersSpinCommand _tryEndPlayersSpinCommand;
        private TryEndStagePreparationPhaseCommand _tryEndStagePreparationPhaseCommand;
        private StepAllPlayersTalentsCooldownsCommand _stepAllPlayersTalentsCooldownsCommand;
        private StepAllPlayersTalentsCommand _stepAllPlayersTalentsCommand;
        private TrySendPlayersLockOnTargetChangedCommand _trySendPlayersLockOnTargetChangedCommand;
        
        private readonly MatchFullTickPacketS2C _fullTickPacket;
        private readonly StartMatchPacketS2C _cachedStartMatchPacket;
        private readonly StartStagePacketS2C _startStagePacket;

        public ServerMatchNetworkTickProcessor(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, IServerNetworkManager networkManager,
            IMatchPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchDataService matchDataService,
            IMatchPlayerJoinPacketsHandler iIMatchPlayerJoinPacketsHandler, INetEventsDataService netEventsDataService,
            ICommandFactory commandFactory, ITickService tickService, IHeadLessQuitterController headLessQuitterController, IStageDataService stageDataService, IOverrideableNetEventsService overrideableNetEventsService, IClientsNetworkDataService clientsNetworkDataService,
            IPlayersPowerUpsManager playersPowerUpsManager)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchDataService = matchDataService;
            _matchPlayerJoinPacketsHandler = iIMatchPlayerJoinPacketsHandler;
            _netEventsDataService = netEventsDataService;
            _commandFactory = commandFactory;
            _tickService = tickService;
            _headLessQuitterController = headLessQuitterController;
            _stageDataService = stageDataService;
            _overrideableNetEventsService = overrideableNetEventsService;
            _clientsNetworkDataService = clientsNetworkDataService;
            _playersPowerUpsManager = playersPowerUpsManager;
            _fullTickPacket = new MatchFullTickPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig);
            _cachedStartMatchPacket = new StartMatchPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount);
            _startStagePacket = new StartStagePacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount);
        }

        public void InitEntryPoint()
        {
            _tryEndPlayersSpinCommand = _commandFactory.CreateCommandVoid<TryEndPlayersSpinCommand>();
            _tryDamagePlayersInLavaCommand = _commandFactory.CreateCommandVoid<TryDamagePlayersInLavaCommand>();
            _trySpawnPowerUpBallsCommand = _commandFactory.CreateCommandVoid<TrySpawnPowerUpBallsCommand>();
            _stepTimersCommand = _commandFactory.CreateCommandVoid<StepTimersCommand>();
            _stepPhysiscsSimulationCommand = _commandFactory.CreateCommandVoid<StepPhysiscsSimulationCommand>();
            _stepFrigidBlocksCommand = _commandFactory.CreateCommandVoid<StepFrigidBlocksCommand>();
            _tryEndStagePreparationPhaseCommand = _commandFactory.CreateCommandVoid<TryEndStagePreparationPhaseCommand>();
            _stepAllPlayersTalentsCooldownsCommand = _commandFactory.CreateCommandVoid<StepAllPlayersTalentsCooldownsCommand>();
            _stepAllPlayersTalentsCommand = _commandFactory.CreateCommandVoid<StepAllPlayersTalentsCommand>();
            _trySendPlayersLockOnTargetChangedCommand = _commandFactory.CreateCommandVoid<TrySendPlayersLockOnTargetChangedCommand>();
            _applyGalacticPullForcesCommand = _commandFactory.CreateCommandVoid<ApplyGalacticPullForcesCommand>();
            _tryDeactivateEndedGalacticFieldsCommand = _commandFactory.CreateCommandVoid<TryDeactivateEndedGalacticFieldsCommand>();
            _tickService.RegisterObserver(this);
        }

        public void InitExitPoint()
        {
            _tickService.UnregisterObserver(this);
        }
        
        public void OnTick(int currentTick)
        {
            try
            {
                _networkManager.PollEvents();
                var stepDeltaTime = _networkConfig.DeltaTime;

                if (TryHandleStageEnded(currentTick, stepDeltaTime))
                {
                    return;
                }
                
                _stepTimersCommand.SetStepDeltaTime(stepDeltaTime).Execute();
                _stepAllPlayersTalentsCooldownsCommand.SetStepTick(currentTick).SetStepDeltaTime(stepDeltaTime).Execute();
                var processPlayersInputsResult = ProcessPackets(currentTick, stepDeltaTime);
                _playersPowerUpsManager.OnTick(currentTick);
                _stepAllPlayersTalentsCommand.SetStepTick(currentTick).SetStepDeltaTime(stepDeltaTime).Execute();
                _trySpawnPowerUpBallsCommand.SetProcessedTick(currentTick).Execute();
                _tryDeactivateEndedGalacticFieldsCommand.SetTick(currentTick).Execute();
                _applyGalacticPullForcesCommand.Execute();
                _tryEndStagePreparationPhaseCommand.SetProcessedTick(currentTick).Execute();
                _stepPhysiscsSimulationCommand.SetDeltaTime(stepDeltaTime).SetTick(currentTick).Execute();
                _stepFrigidBlocksCommand.SetTick(currentTick).SetDeltaTime(stepDeltaTime).Execute();
                _tryEndPlayersSpinCommand.SetTick(currentTick).Execute();
                _tryDamagePlayersInLavaCommand.SetProcessedTick(currentTick).Execute();
                _trySendPlayersLockOnTargetChangedCommand.SetProcessedTick(currentTick).Execute();
                _overrideableNetEventsService.RegisterAllOverridableNetEvents();
                RemoveOlderThanTickEventsPerClient(processPlayersInputsResult.HeighestProcessedTickPerClient);
                SendCurrentTickStateToAllClients(currentTick);
                SendStartMatchToNotAcknowledgedClients(currentTick);
                _headLessQuitterController.QuitIfTimeOut();
            }
            catch (Exception e)
            {
                LogService.LogError("Got error! " + e);
                throw;
            }
        }

        private bool TryHandleStageEnded(int currentTick, float stepDeltaTime)
        {
            if (!_stageDataService.IsStageEnded)
            {
                return false;
            }

            _stageDataService.StageRestartTimer -= stepDeltaTime;
            var didRestartTimerEnded = _stageDataService.StageRestartTimer <= 0;
            if (!didRestartTimerEnded)
            {
                return false;
            }

            _commandFactory.CreateCommandVoid<InitStageCommand>().Execute();
            SendStartStageToAllClients(currentTick);
            return true;
        }

        private void SendStartStageToAllClients(int processedTick)
        {
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                if (!kvp.Value.IsConnected)
                {
                    return;
                }

                var clientId = kvp.Key;
                SendStartStagePacketToClient(clientId, processedTick, DeliveryMethod.ReliableUnordered); // we don't send in a net event cause it can fill quickly our packet buffer
            }
        }
        
        private void SendStartStagePacketToClient(long clientId, int processedTick, DeliveryMethod deliveryMethod)
        {
            _startStagePacket.InitialState = _matchDataService.SimulationState;
            _startStagePacket.OccuredOnTick = processedTick;
            _networkManager.SendPacketToClientSerialized(clientId, PacketTypeS2C.StartStage, _startStagePacket, deliveryMethod);
        }
        
        private void SendStartMatchToNotAcknowledgedClients(int processedTick)
        {
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                var clientId = kvp.Key;
                var clientNetworkData = kvp.Value;
                var didClientAcknowledgeMatch = _playerInputsPacketsHandler.DidReceiveAnyInputFromClient(clientId);
                if (!didClientAcknowledgeMatch && clientNetworkData.IsConnected)
                {
                    SendStartMatchPacketToClient(clientId, processedTick, DeliveryMethod.Unreliable);
                }
            }
        }

        private void SendStartMatchPacketToClient(long clientId, int processedTick, DeliveryMethod deliveryMethod)
        {
            _cachedStartMatchPacket.InitialState = _matchDataService.SimulationState;
            _cachedStartMatchPacket.OccuredOnTick = processedTick;
            _networkManager.SendPacketToClientSerialized(clientId, PacketTypeS2C.StartMatch, _cachedStartMatchPacket, deliveryMethod);
        }

        private ProcessPlayersInputsResult ProcessPackets(int processedTick, float deltaTime)
        {
            _matchPlayerJoinPacketsHandler.ProcessPlayersJoined(processedTick);
            return _playerInputsPacketsHandler.ProcessInputs(processedTick, deltaTime);
        }

        private void RemoveOlderThanTickEventsPerClient(CapacityDict<long, int> heighestProcessedTickPerClient)
        {
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                var clientId = kvp.Key;

                if (heighestProcessedTickPerClient.TryGetValue(clientId, out int tickOfClient))
                {
                    _netEventsDataService.RemoveAllEventsOlderThanTick(clientId, tickOfClient);
                }
            }
        }

        private void SendCurrentTickStateToAllClients(int processedTick)
        {
            if (_matchDataService.SimulationState.Players.Count == 0)
            {
                return;
            }

            var currentSimulationState = _matchDataService.SimulationState;
            _fullTickPacket.Tick = processedTick;
            _fullTickPacket.CurrentSimulationState = currentSimulationState;

            //_fullTickPacket.PreviousSimulationState = _matchDataService.PreviousSimulationState;
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                if (!kvp.Value.IsConnected)
                {
                    return;
                }

                var clientId = kvp.Key;
                _fullTickPacket.BulletSpawnNetEvents = _netEventsDataService.BulletSpawnNetEventsPerClient[clientId];
                _fullTickPacket.PlayerJoinAcceptNetEvents = _netEventsDataService.PlayerRejoinAcceptNetEventsPerClient[clientId];
                _fullTickPacket.PlayerTakeDamageNetEvents = _netEventsDataService.PlayerTakeDamageNetEventsPerClient[clientId];
                _fullTickPacket.PlayerDiedNetEvents = _netEventsDataService.PlayerDiedNetEventsPerClient[clientId];
                _fullTickPacket.BulletDestroyedNetEvents = _netEventsDataService.BulletDestroyedNetEventsPerClient[clientId];
                _fullTickPacket.PlayerSwapNetEvents = _netEventsDataService.PlayerSwapNetEventsPerClient[clientId];
                _fullTickPacket.TalentCardObtainedNetEvents = _netEventsDataService.TalentCardObtainedNetEventsPerClient[clientId];
                _fullTickPacket.TalentCardHitNetEvents = _netEventsDataService.TalentCardHitNetEventsPerClient[clientId];
                _fullTickPacket.PlayerSpinnedStartedNetEvents = _netEventsDataService.PlayerSpinnedStartedNetEventsPerClient[clientId];
                _fullTickPacket.PlayerSpinnedEndedNetEvents = _netEventsDataService.PlayerSpinnedEndedNetEventsPerClient[clientId];
                _fullTickPacket.PowerUpSpawnedNetEvents = _netEventsDataService.PowerUpBallSpawnedNetEventsPerClient[clientId];
                _fullTickPacket.PowerUpObtainedNetEvents = _netEventsDataService.PowerUpBallObtainedNetEventsPerClient[clientId];
                _fullTickPacket.StageEndNetEvents = _netEventsDataService.StageEndNetEventsPerClient[clientId];
                _fullTickPacket.TeamLostNetEvents = _netEventsDataService.TeamLostNetEventsPerClient[clientId];
                _fullTickPacket.TalentSwitchNetEvents = _netEventsDataService.TalentSwitchNetEventsPerClient[clientId];
                _fullTickPacket.EnvironmentSpringPlayerCollisionNetEvents = _netEventsDataService.EnvironmentSpringPlayerCollisionNetEventsPerClient[clientId];
                _fullTickPacket.EnvironmentSpikePlayerCollisionNetEvents = _netEventsDataService.EnvironmentSpikePlayerCollisionNetEventsPerClient[clientId];
                _fullTickPacket.GainBoltsNetEvents = _netEventsDataService.GainBoltsNetEventsPerClient[clientId];
                _fullTickPacket.PlayerToEnvironmentTeleportGateCollisionNetEvents = _netEventsDataService.PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient[clientId];
                _fullTickPacket.PreparationPhaseEndedNetEvents = _netEventsDataService.PreparationPhaseEndedNetEventsPerClient[clientId];
                _fullTickPacket.CreateSwapFieldNetEvents = _netEventsDataService.CreateSwapFieldNetEventsPerClient[clientId];
                _fullTickPacket.CreateKOProjectileNetEvents = _netEventsDataService.CreateKOProjectileNetEventsPerClient[clientId];
                _fullTickPacket.KOProjectHitPlayerNetEvents = _netEventsDataService.KOProjectHitPlayerNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateKOTalentNetEvents = _netEventsDataService.DeactivateKOTalentNetEventsPerClient[clientId];
                _fullTickPacket.ActivateSentryGunTalentNetEvents = _netEventsDataService.ActivateSentryGunTalentNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateSentryGunTalentNetEvents = _netEventsDataService.DeactivateSentryGunTalentNetEventsPerClient[clientId];
                _fullTickPacket.PerformDashPulseNetEvents = _netEventsDataService.PerformDashPulseNetEventsPerClient[clientId];
                _fullTickPacket.UpdatePlayerTalentStocksNetEvents = _netEventsDataService.UpdatePlayerTalentStocksNetEventsPerClient[clientId];
                _fullTickPacket.PlayerMaxShootCooldownChangedNetEvents = _netEventsDataService.PlayerMaxShootCooldownChangedNetEventsPerClient[clientId];
                _fullTickPacket.DestroySwapFieldNetEvents = _netEventsDataService.DeactivateSwapTalentNetEventsPerClient[clientId];
                _fullTickPacket.CreateGrapplingHookProjectileNetEvents = _netEventsDataService.CreateGrapplingHookProjectileNetEventsPerClient[clientId];
                _fullTickPacket.GrapplingHookHitWallNetEvents = _netEventsDataService.GrapplingHookHitWallNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateGrapplingHookTalentNetEvents = _netEventsDataService.DeactivateGrapplingHookTalentNetEventsPerClient[clientId];
                _fullTickPacket.ActivateUmbrellaTalentNetEvents = _netEventsDataService.ActivateUmbrellaTalentNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateUmbrellaTalentNetEvents = _netEventsDataService.DeactivateUmbrellaTalentNetEventsPerClient[clientId];
                _fullTickPacket.ActivateWaterGunTalentNetEvents = _netEventsDataService.ActivateWaterGunTalentNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateWaterGunTalentNetEvents = _netEventsDataService.DeactivateWaterGunTalentNetEventsPerClient[clientId];
                _fullTickPacket.ActivateHeadbuttChargingNetEvents = _netEventsDataService.ActivateHeadbuttChargingNetEventsPerClient[clientId];
                _fullTickPacket.PerformHeadbuttDashNetEvents = _netEventsDataService.PerformHeadbuttDashNetEventsPerClient[clientId];
                _fullTickPacket.HeadbuttHitEnemyNetEvents = _netEventsDataService.HeadbuttHitEnemyNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateHeadbuttTalentNetEvents = _netEventsDataService.DeactivateHeadbuttTalentNetEventsPerClient[clientId];
                _fullTickPacket.CreateMagneticPullFieldNetEvents = _netEventsDataService.CreateMagneticPullFieldNetEventsPerClient[clientId];
                _fullTickPacket.LayChickenEggNetEvents = _netEventsDataService.LayChickenEggNetEventsPerClient[clientId];
                _fullTickPacket.ChickenEggHitNetEvents = _netEventsDataService.ChickenEggHitNetEventsPerClient[clientId];
                _fullTickPacket.ActivateYearsOfPainTalentNetEvents = _netEventsDataService.ActivateYearsOfPainTalentNetEventsPerClient[clientId];
                _fullTickPacket.PlayerLockOnTargetsChangedNetEvents = _netEventsDataService.PlayerLockOnTargetsChangedNetEventsPerClient[clientId];
                _fullTickPacket.PlayerLockedOnTargetHitNetEvents = _netEventsDataService.PlayerLockedOnTargetHitNetEventsPerClient[clientId];
                _fullTickPacket.PlayerPowerUpChangedNetEvents = _netEventsDataService.PlayerPowerUpChangedNetEventsPerClient[clientId];
                _fullTickPacket.ActivateSonicSlapNetEvents = _netEventsDataService.ActivateSonicSlapNetEventsPerClient[clientId];
                _fullTickPacket.PerformGalacticPullNetEvents = _netEventsDataService.PerformGalacticPullNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateGalacticForceFieldNetEvents = _netEventsDataService.DeactivateGalacticForceFieldNetEventsPerClient[clientId];
                _fullTickPacket.ActivateNukePowerUpNetEvents = _netEventsDataService.ActivateNukePowerUpNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateShufflePowerUpNetEvents = _netEventsDataService.DeactivateShufflePowerUpNetEventsPerClient[clientId];
                _fullTickPacket.ShuffleSwapPlayerPositionNetEvents = _netEventsDataService.ShuffleSwapPlayerPositionNetEventsPerClient[clientId];
                _fullTickPacket.ActivateShuffleNetEvents = _netEventsDataService.ActivateShuffleNetEventsPerClient[clientId];
                _fullTickPacket.StartPowerUpGrantingPhaseNetEvents = _netEventsDataService.StartPowerUpGrantingPhaseNetEventsPerClient[clientId];
                _fullTickPacket.EndPowerUpGrantingPhaseNetEvents = _netEventsDataService.EndPowerUpGrantingPhaseNetEventsPerClient[clientId];
                _fullTickPacket.ShootFrigidBlockNetEvents = _netEventsDataService.ShootFrigidBlockNetEventsPerClient[clientId];
                _fullTickPacket.DestroyFrigidBlockNetEvents = _netEventsDataService.DestroyFrigidBlockNetEventsPerClient[clientId];
                _fullTickPacket.FishingRodThrowNetEvents = _netEventsDataService.FishingRodThrowNetEventsPerClient[clientId];
                _fullTickPacket.FishingRodCaughtEnemyNetEvents = _netEventsDataService.FishingRodCaughtEnemyNetEventsPerClient[clientId];
                _fullTickPacket.FishingRodTipHitWallNetEvents = _netEventsDataService.FishingRodTipHitWallNetEventsPerClient[clientId];
                _fullTickPacket.CreateFishingRodProjectileNetEvents = _netEventsDataService.CreateFishingRodProjectileNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateFishingRodTalentNetEvents = _netEventsDataService.DeactivateFishingRodTalentNetEventsPerClient[clientId];
                _fullTickPacket.CreateSoulGhostNetEvents = _netEventsDataService.CreateSoulGhostNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateSoulTalentNetEvents = _netEventsDataService.DeactivateSoulTalentNetEventsPerClient[clientId];
                _fullTickPacket.ActivateRockTalentNetEvents = _netEventsDataService.ActivateRockTalentNetEventsPerClient[clientId];
                _fullTickPacket.DeactivateRockTalentNetEvents = _netEventsDataService.DeactivateRockTalentNetEventsPerClient[clientId];
                _networkManager.SendPacketToClientSerialized(clientId, PacketTypeS2C.MatchFullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}
