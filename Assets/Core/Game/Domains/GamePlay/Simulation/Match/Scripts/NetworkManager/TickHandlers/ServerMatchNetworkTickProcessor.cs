using System;
using System.Diagnostics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
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
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private readonly IMatchDataService _matchDataService;
        private readonly IMatchPlayerJoinPacketsHandler _matchPlayerJoinPacketsHandler;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly ICommandFactory _commandFactory;
        private readonly ITickService _tickService;
        private readonly IHeadLessQuitterController _headLessQuitterController;
        private readonly IStageDataService _stageDataService;
        private readonly IOverrideableNetEventsService _overrideableNetEventsService;

        private TryDamagePlayersInLavaCommand _tryDamagePlayersInLavaCommand;
        private TrySpawnPowerUpBallsCommand _trySpawnPowerUpBallsCommand;
        private StepPhysiscsSimulationCommand _stepPhysiscsSimulationCommand;
        private StepTimersCommand _stepTimersCommand;
        private readonly MatchFullTickPacketS2C _fullTickPacket;
        private StartMatchPacketS2C _cachedStartMatchPacket;
        private StartStagePacketS2C _startStagePacket;
        private TryEndStagePreparationPhaseCommand _tryEndStagePreparationPhaseCommand;
        private StepAllPlayersTalentsCooldownsCommand _stepAllPlayersTalentsCooldownsCommand;
        //private TimerFixedThreaded2 _pollEventsFixedTimer;
        private Stopwatch _sw;
        private long _last;

        public ServerMatchNetworkTickProcessor(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, IServerNetworkManager networkManager,
            IMatchPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchDataService matchDataService,
            IMatchPlayerJoinPacketsHandler iIMatchPlayerJoinPacketsHandler, INetEventsDataService iNetEventsDataService, IPhysicsSimulator physicsSimulator,
            ICommandFactory commandFactory, ITickService tickService, IHeadLessQuitterController headLessQuitterController, IStageDataService stageDataService, IOverrideableNetEventsService overrideableNetEventsService)
        {
            _networkConfig = networkConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkManager = networkManager;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchDataService = matchDataService;
            _matchPlayerJoinPacketsHandler = iIMatchPlayerJoinPacketsHandler;
            _netEventsDataService = iNetEventsDataService;
            _physicsSimulator = physicsSimulator;
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
            _tryDamagePlayersInLavaCommand = _commandFactory.CreateCommandVoid<TryDamagePlayersInLavaCommand>();
            _trySpawnPowerUpBallsCommand = _commandFactory.CreateCommandVoid<TrySpawnPowerUpBallsCommand>();
            _stepTimersCommand = _commandFactory.CreateCommandVoid<StepTimersCommand>();
            _stepPhysiscsSimulationCommand = _commandFactory.CreateCommandVoid<StepPhysiscsSimulationCommand>();
            _tryEndStagePreparationPhaseCommand = _commandFactory.CreateCommandVoid<TryEndStagePreparationPhaseCommand>();
            _stepAllPlayersTalentsCooldownsCommand = _commandFactory.CreateCommandVoid<StepAllPlayersTalentsCooldownsCommand>();
            _tickService.RegisterObserver(this);
        }

        private void PollEvents()
        {
            // _sw = Stopwatch.StartNew();
            // _last = _sw.ElapsedMilliseconds;
            //
            //     long now = _sw.ElapsedMilliseconds;
            //     long dt = now - _last;
            //     _last = now;

                //if (dt > 20)
              //      Debug.LogError($"PollEvents stall: {dt}ms");

            // In playback, we DO poll events, but the NetworkManager handles fetching from PlaybackService
            //_networkManager.PollEvents();
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

                TryHandleStageEnded(currentTick, stepDeltaTime);
                _stepTimersCommand.SetStepDeltaTime(stepDeltaTime).Execute();
                _stepAllPlayersTalentsCooldownsCommand.SetStepTick(currentTick).SetStepDeltaTime(stepDeltaTime).Execute();
                var processPlayersInputsResult = ProcessPackets(currentTick, stepDeltaTime);
                _trySpawnPowerUpBallsCommand.SetProcessedTick(currentTick).Execute();
                _tryEndStagePreparationPhaseCommand.SetProcessedTick(currentTick).Execute();
                _stepPhysiscsSimulationCommand.SetDeltaTime(stepDeltaTime).SetTick(currentTick).Execute();
                _tryDamagePlayersInLavaCommand.SetProcessedTick(currentTick).Execute();
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

        private void TryHandleStageEnded(int currentTick, float stepDeltaTime)
        {
            if (!_stageDataService.IsStageEnded)
            {
                return;
            }

            _stageDataService.StageRestartTimer -= stepDeltaTime;
            var didRestartTimerEnded = _stageDataService.StageRestartTimer <= 0;
            if (!didRestartTimerEnded)
            {
                return;
            }

            _commandFactory.CreateCommandVoid<InitStageCommand>().Execute();
            SendStartStageToAllPlayers(currentTick);// why do we send this and not add it as a NetEvent???
        }

        private void SendStartStageToAllPlayers(int processedTick)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!playerState.IsConnected)
                {
                    return;
                }
                
                SendStartStagePacketToClient(playerState.Id, processedTick, DeliveryMethod.ReliableUnordered);
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
                _fullTickPacket.BulletSpawnNetEvents = _netEventsDataService.BulletSpawnNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerJoinAcceptNetEvents = _netEventsDataService.PlayerRejoinAcceptNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerTakeDamageNetEvents = _netEventsDataService.PlayerTakeDamageNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerDiedNetEvents = _netEventsDataService.PlayerDiedNetEventsPerPlayer[playerId];
                _fullTickPacket.BulletDestroyedNetEvents = _netEventsDataService.BulletDestroyedNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerSwapNetEvents = _netEventsDataService.PlayerSwapNetEventsPerPlayer[playerId];
                _fullTickPacket.TalentCardObtainedNetEvents = _netEventsDataService.TalentCardObtainedNetEventsPerPlayer[playerId];
                _fullTickPacket.TalentCardHitNetEvents = _netEventsDataService.TalentCardHitNetEventsPerPlayer[playerId];
                _fullTickPacket.PowerUpSpawnedNetEvents = _netEventsDataService.PowerUpBallSpawnedNetEventsPerPlayer[playerId];
                _fullTickPacket.PowerUpObtainedNetEvents = _netEventsDataService.PowerUpBallObtainedNetEventsPerPlayer[playerId];
                _fullTickPacket.StageEndNetEvents = _netEventsDataService.StageEndNetEventsPerPlayer[playerId];
                _fullTickPacket.TeamLostNetEvents = _netEventsDataService.TeamLostNetEventsPerPlayer[playerId];
                _fullTickPacket.TalentSwitchNetEvents = _netEventsDataService.TalentSwitchNetEventsPerPlayer[playerId];
                _fullTickPacket.EnvironmentSpringPlayerCollisionNetEvents = _netEventsDataService.EnvironmentSpringPlayerCollisionNetEventsPerPlayer[playerId];
                _fullTickPacket.GainBoltsNetEvents = _netEventsDataService.GainBoltsNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerToEnvironmentTeleportGateCollisionNetEvents = _netEventsDataService.PlayerToEnvironmentTeleportGateCollisionNetEventsPerPlayer[playerId];
                _fullTickPacket.PreparationPhaseEndedNetEvents = _netEventsDataService.PreparationPhaseEndedNetEventsPerPlayer[playerId];
                _fullTickPacket.CreateSwapFieldNetEvents = _netEventsDataService.CreateSwapFieldNetEventsPerPlayer[playerId];
                _fullTickPacket.CreateKOProjectileNetEvents = _netEventsDataService.CreateKOProjectileNetEventsPerPlayer[playerId];
                _fullTickPacket.KOProjectHitPlayerNetEvents = _netEventsDataService.KOProjectHitPlayerNetEventsPerPlayer[playerId];
                _fullTickPacket.DeactivateKOTalentNetEvents = _netEventsDataService.DeactivateKOTalentNetEventsPerPlayer[playerId];
                _fullTickPacket.ActivateSentryGunTalentNetEvents = _netEventsDataService.ActivateSentryGunTalentNetEventsPerPlayer[playerId];
                _fullTickPacket.DeactivateSentryGunTalentNetEvents = _netEventsDataService.DeactivateSentryGunTalentNetEventsPerPlayer[playerId];
                _fullTickPacket.PerformDashPulseNetEvents = _netEventsDataService.PerformDashPulseNetEventsPerPlayer[playerId];
                _fullTickPacket.UpdatePlayerTalentStocksNetEvents = _netEventsDataService.UpdatePlayerTalentStocksNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerMaxShootCooldownChangedNetEvents = _netEventsDataService.PlayerMaxShootCooldownChangedNetEventsPerPlayer[playerId];
                _fullTickPacket.DestroySwapFieldNetEvents = _netEventsDataService.DeactivateSwapTalentNetEventsPerPlayer[playerId];
                _fullTickPacket.CreateGrapplingHookProjectileNetEvents = _netEventsDataService.CreateGrapplingHookProjectileNetEventsPerPlayer[playerId];
                _fullTickPacket.GrapplingHookHitWallNetEvents = _netEventsDataService.GrapplingHookHitWallNetEventsPerPlayer[playerId];
                _fullTickPacket.DeactivateGrapplingHookTalentNetEvents = _netEventsDataService.DeactivateGrapplingHookTalentNetEventsPerPlayer[playerId];
                _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.MatchFullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}
