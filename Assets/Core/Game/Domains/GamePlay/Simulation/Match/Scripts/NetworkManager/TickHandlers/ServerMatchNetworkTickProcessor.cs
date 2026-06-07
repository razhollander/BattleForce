using System;
using System.Diagnostics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
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

        private TryDamagePlayersInLavaCommand _tryDamagePlayersInLavaCommand;
        private TryDamagePlayersOnTargetCommand _tryDamagePlayersOnTargetCommand;
        private TrySpawnPowerUpBallsCommand _trySpawnPowerUpBallsCommand;
        private StepPhysiscsSimulationCommand _stepPhysiscsSimulationCommand;
        private StepTimersCommand _stepTimersCommand;
        private TryEndPlayersSpinCommand _tryEndPlayersSpinCommand;
        private TryEndStagePreparationPhaseCommand _tryEndStagePreparationPhaseCommand;
        private StepAllPlayersTalentsCooldownsCommand _stepAllPlayersTalentsCooldownsCommand;
        private TrySendPlayersLockOnTargetChangedCommand _trySendPlayersLockOnTargetChangedCommand;
        
        private readonly MatchFullTickPacketS2C _fullTickPacket;
        private readonly StartMatchPacketS2C _cachedStartMatchPacket;
        private readonly StartStagePacketS2C _startStagePacket;

        public ServerMatchNetworkTickProcessor(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, IServerNetworkManager networkManager,
            IMatchPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchDataService matchDataService,
            IMatchPlayerJoinPacketsHandler iIMatchPlayerJoinPacketsHandler, INetEventsDataService netEventsDataService,
            ICommandFactory commandFactory, ITickService tickService, IHeadLessQuitterController headLessQuitterController, IStageDataService stageDataService, IOverrideableNetEventsService overrideableNetEventsService)
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
            _fullTickPacket = new MatchFullTickPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig);
            _cachedStartMatchPacket = new StartMatchPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount);
            _startStagePacket = new StartStagePacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount);
        }

        public void InitEntryPoint()
        {
            _tryEndPlayersSpinCommand = _commandFactory.CreateCommandVoid<TryEndPlayersSpinCommand>();
            _tryDamagePlayersInLavaCommand = _commandFactory.CreateCommandVoid<TryDamagePlayersInLavaCommand>();
            _tryDamagePlayersOnTargetCommand = _commandFactory.CreateCommandVoid<TryDamagePlayersOnTargetCommand>();
            _trySpawnPowerUpBallsCommand = _commandFactory.CreateCommandVoid<TrySpawnPowerUpBallsCommand>();
            _stepTimersCommand = _commandFactory.CreateCommandVoid<StepTimersCommand>();
            _stepPhysiscsSimulationCommand = _commandFactory.CreateCommandVoid<StepPhysiscsSimulationCommand>();
            _tryEndStagePreparationPhaseCommand = _commandFactory.CreateCommandVoid<TryEndStagePreparationPhaseCommand>();
            _stepAllPlayersTalentsCooldownsCommand = _commandFactory.CreateCommandVoid<StepAllPlayersTalentsCooldownsCommand>();
            _trySendPlayersLockOnTargetChangedCommand = _commandFactory.CreateCommandVoid<TrySendPlayersLockOnTargetChangedCommand>();
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
                _trySpawnPowerUpBallsCommand.SetProcessedTick(currentTick).Execute();
                _tryEndStagePreparationPhaseCommand.SetProcessedTick(currentTick).Execute();
                _stepPhysiscsSimulationCommand.SetDeltaTime(stepDeltaTime).SetTick(currentTick).Execute();
                _tryEndPlayersSpinCommand.SetTick(currentTick).Execute();
                _tryDamagePlayersInLavaCommand.SetProcessedTick(currentTick).Execute();
                _trySendPlayersLockOnTargetChangedCommand.SetProcessedTick(currentTick).Execute();
                _tryDamagePlayersOnTargetCommand.SetProcessedTick(currentTick).Execute();
                _overrideableNetEventsService.RegisterAllOverridableNetEvents();
                RemoveOlderThanTickEventsPerPlayer(processPlayersInputsResult.HeighestProcessedTickPerPlayer);
                SendCurrentTickStateToAllClients(currentTick);
                SendStartMatchToNotAcknowledgedPlayers(currentTick);
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
            SendStartStageToAllPlayers(currentTick);
            return true;
        }

        private void SendStartStageToAllPlayers(int processedTick)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!playerState.IsConnected)
                {
                    return;
                }
                
                SendStartStagePacketToClient(playerState.Id, processedTick, DeliveryMethod.ReliableUnordered); // we don't send in a net event cause it can fill quickly our packet buffer
            }
        }
        
        private void SendStartStagePacketToClient(ushort playerId, int processedTick, DeliveryMethod deliveryMethod)
        {
            _startStagePacket.InitialState = _matchDataService.SimulationState;
            _startStagePacket.OccuredOnTick = processedTick;
            _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.StartStage, _startStagePacket, deliveryMethod);
        }
        
        private void SendStartMatchToNotAcknowledgedPlayers(int processedTick)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var didPlayerAcknowledgeMatch = _playerInputsPacketsHandler.DidReceiveAnyInputFromPlayer(playerState.Id);
                if (!didPlayerAcknowledgeMatch && playerState.IsConnected)
                {
                    SendStartMatchPacketToClient(playerState.Id, processedTick, DeliveryMethod.Unreliable);
                }
            }
        }

        private void SendStartMatchPacketToClient(ushort playerId, int processedTick, DeliveryMethod deliveryMethod)
        {
            _cachedStartMatchPacket.InitialState = _matchDataService.SimulationState;
            _cachedStartMatchPacket.OccuredOnTick = processedTick;
            _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.StartMatch, _cachedStartMatchPacket, deliveryMethod);
        }

        private ProcessPlayersInputsResult ProcessPackets(int processedTick, float deltaTime)
        {
            _matchPlayerJoinPacketsHandler.ProcessPlayersJoined(processedTick);
            return _playerInputsPacketsHandler.ProcessInputs(processedTick, deltaTime);
        }

        private void RemoveOlderThanTickEventsPerPlayer(CapacityDict<ushort, int> heighestProcessedTickPerPlayer)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())        
            {
                var playerId = playerState.Id;

                if (heighestProcessedTickPerPlayer.TryGetValue(playerId, out int tickOfPlayer))
                {
                    _netEventsDataService.RemoveAllEventsOlderThanTick(playerId, tickOfPlayer);
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
            foreach (var playerState in currentSimulationState.Players.AsSpan())
            {
                if (!playerState.IsConnected)
                {
                    return;
                }
                
                var playerId = playerState.Id;
                _fullTickPacket.BulletSpawnNetEvents = _netEventsDataService.BulletSpawnNetEventsPerClient[playerId];
                _fullTickPacket.PlayerJoinAcceptNetEvents = _netEventsDataService.PlayerRejoinAcceptNetEventsPerClient[playerId];
                _fullTickPacket.PlayerTakeDamageNetEvents = _netEventsDataService.PlayerTakeDamageNetEventsPerClient[playerId];
                _fullTickPacket.PlayerDiedNetEvents = _netEventsDataService.PlayerDiedNetEventsPerClient[playerId];
                _fullTickPacket.BulletDestroyedNetEvents = _netEventsDataService.BulletDestroyedNetEventsPerClient[playerId];
                _fullTickPacket.PlayerSwapNetEvents = _netEventsDataService.PlayerSwapNetEventsPerClient[playerId];
                _fullTickPacket.TalentCardObtainedNetEvents = _netEventsDataService.TalentCardObtainedNetEventsPerClient[playerId];
                _fullTickPacket.TalentCardHitNetEvents = _netEventsDataService.TalentCardHitNetEventsPerClient[playerId];
                _fullTickPacket.PlayerSpinnedStartedNetEvents = _netEventsDataService.PlayerSpinnedStartedNetEventsPerClient[playerId];
                _fullTickPacket.PlayerSpinnedEndedNetEvents = _netEventsDataService.PlayerSpinnedEndedNetEventsPerClient[playerId];
                _fullTickPacket.PowerUpSpawnedNetEvents = _netEventsDataService.PowerUpBallSpawnedNetEventsPerClient[playerId];
                _fullTickPacket.PowerUpObtainedNetEvents = _netEventsDataService.PowerUpBallObtainedNetEventsPerClient[playerId];
                _fullTickPacket.StageEndNetEvents = _netEventsDataService.StageEndNetEventsPerClient[playerId];
                _fullTickPacket.TeamLostNetEvents = _netEventsDataService.TeamLostNetEventsPerClient[playerId];
                _fullTickPacket.TalentSwitchNetEvents = _netEventsDataService.TalentSwitchNetEventsPerClient[playerId];
                _fullTickPacket.EnvironmentSpringPlayerCollisionNetEvents = _netEventsDataService.EnvironmentSpringPlayerCollisionNetEventsPerClient[playerId];
                _fullTickPacket.GainBoltsNetEvents = _netEventsDataService.GainBoltsNetEventsPerClient[playerId];
                _fullTickPacket.PlayerToEnvironmentTeleportGateCollisionNetEvents = _netEventsDataService.PlayerToEnvironmentTeleportGateCollisionNetEventsPerClient[playerId];
                _fullTickPacket.PreparationPhaseEndedNetEvents = _netEventsDataService.PreparationPhaseEndedNetEventsPerClient[playerId];
                _fullTickPacket.CreateSwapFieldNetEvents = _netEventsDataService.CreateSwapFieldNetEventsPerClient[playerId];
                _fullTickPacket.CreateKOProjectileNetEvents = _netEventsDataService.CreateKOProjectileNetEventsPerClient[playerId];
                _fullTickPacket.KOProjectHitPlayerNetEvents = _netEventsDataService.KOProjectHitPlayerNetEventsPerClient[playerId];
                _fullTickPacket.DeactivateKOTalentNetEvents = _netEventsDataService.DeactivateKOTalentNetEventsPerClient[playerId];
                _fullTickPacket.ActivateSentryGunTalentNetEvents = _netEventsDataService.ActivateSentryGunTalentNetEventsPerClient[playerId];
                _fullTickPacket.DeactivateSentryGunTalentNetEvents = _netEventsDataService.DeactivateSentryGunTalentNetEventsPerClient[playerId];
                _fullTickPacket.PerformDashPulseNetEvents = _netEventsDataService.PerformDashPulseNetEventsPerClient[playerId];
                _fullTickPacket.UpdatePlayerTalentStocksNetEvents = _netEventsDataService.UpdatePlayerTalentStocksNetEventsPerClient[playerId];
                _fullTickPacket.PlayerMaxShootCooldownChangedNetEvents = _netEventsDataService.PlayerMaxShootCooldownChangedNetEventsPerClient[playerId];
                _fullTickPacket.DestroySwapFieldNetEvents = _netEventsDataService.DeactivateSwapTalentNetEventsPerClient[playerId];
                _fullTickPacket.CreateGrapplingHookProjectileNetEvents = _netEventsDataService.CreateGrapplingHookProjectileNetEventsPerClient[playerId];
                _fullTickPacket.GrapplingHookHitWallNetEvents = _netEventsDataService.GrapplingHookHitWallNetEventsPerClient[playerId];
                _fullTickPacket.DeactivateGrapplingHookTalentNetEvents = _netEventsDataService.DeactivateGrapplingHookTalentNetEventsPerClient[playerId];
                _fullTickPacket.ActivateUmbrellaTalentNetEvents = _netEventsDataService.ActivateUmbrellaTalentNetEventsPerClient[playerId];
                _fullTickPacket.DeactivateUmbrellaTalentNetEvents = _netEventsDataService.DeactivateUmbrellaTalentNetEventsPerClient[playerId];
                _fullTickPacket.CreateMagneticPullFieldNetEvents = _netEventsDataService.CreateMagneticPullFieldNetEventsPerClient[playerId];
                _fullTickPacket.LayChickenEggNetEvents = _netEventsDataService.LayChickenEggNetEventsPerClient[playerId];
                _fullTickPacket.ChickenEggHitNetEvents = _netEventsDataService.ChickenEggHitNetEventsPerClient[playerId];
                _fullTickPacket.ActivateYearsOfPainTalentNetEvents = _netEventsDataService.ActivateYearsOfPainTalentNetEventsPerClient[playerId];
                _fullTickPacket.PlayerLockOnHeartTargetsChangedNetEvents = _netEventsDataService.PlayerLockOnHeartTargetsChangedNetEventsPerClient[playerId];
                _fullTickPacket.PlayerLockedOnTargetHitNetEvents = _netEventsDataService.PlayerLockedOnTargetHitNetEventsPerClient[playerId];
                _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.MatchFullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}
