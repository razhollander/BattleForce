using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.States;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager
{
    public class ServerMatchMakingNetworkTickProcessor: ITickProcessor
    {
        private readonly NetworkConfig _networkConfig;
        private readonly IServerNetworkManager _networkManager;
        private readonly IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private readonly IMatchMakingDataService _matchMakingDataService;
        private readonly IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IStateMachineService _stateMachineService;
        private readonly ICommandFactory _commandFactory;
        private readonly ITickService _tickService;
        private readonly IStartMatchWallController _startMatchWallController;
        private readonly ISimulationStateMachine _simulationStateMachine;
        private readonly IHeadLessQuitterController _headLessQuitterController;
        private readonly IClientsNetworkDataService _clientsNetworkDataService;

        private StepTimersCommand _stepTimersCommand;
        private StepPhysiscsSimulationCommand _stepPhysiscsSimulationCommand;
        private MatchMakingFullTickPacketS2C _fullTickPacket;
        private HandleIfAnyPlayerChangedTeamFloorCommand _handleIfAnyPlayerChangedTeamFloorCommand;
        private HandleIfStartMatchEligiblityChangedCommand _handleIfStartMatchEligiblityChangedCommand;
        private UpdatePlayersLockOnWallStateCommand _updatePlayersLockOnWallStateCommand;
        private Stopwatch _sw;
        private long _last;

        public ServerMatchMakingNetworkTickProcessor(NetworkConfig networkConfig, IServerNetworkManager networkManager,
            IPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchMakingDataService matchMakingDataService,
            IPlayerJoinPacketsHandler playerJoinPacketsHandler, INetEventsDataService netEventsDataService,
            ICommandFactory commandFactory, ITickService tickService, IStartMatchWallController startMatchWallController, ISimulationStateMachine simulationStateMachine,
            IHeadLessQuitterController headLessQuitterController, IClientsNetworkDataService clientsNetworkDataService)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchMakingDataService = matchMakingDataService;
            _playerJoinPacketsHandler = playerJoinPacketsHandler;
            _netEventsDataService = netEventsDataService;
            _commandFactory = commandFactory;
            _tickService = tickService;
            _startMatchWallController = startMatchWallController;
            _simulationStateMachine = simulationStateMachine;
            _headLessQuitterController = headLessQuitterController;
            _clientsNetworkDataService = clientsNetworkDataService;
        }

        public void InitEntryPoint()
        {
            _fullTickPacket = new MatchMakingFullTickPacketS2C();
            _stepTimersCommand = _commandFactory.CreateCommandVoid<StepTimersCommand>();
            _stepPhysiscsSimulationCommand = _commandFactory.CreateCommandVoid<StepPhysiscsSimulationCommand>();
            _handleIfAnyPlayerChangedTeamFloorCommand = _commandFactory.CreateCommandVoid<HandleIfAnyPlayerChangedTeamFloorCommand>();
            _handleIfStartMatchEligiblityChangedCommand = _commandFactory.CreateCommandVoid<HandleIfStartMatchEligiblityChangedCommand>();
            _updatePlayersLockOnWallStateCommand = _commandFactory.CreateCommandVoid<UpdatePlayersLockOnWallStateCommand>();
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
                _stepTimersCommand.SetStepDeltaTime(stepDeltaTime).Execute();
                var processPlayersInputsResult = ProcessPackets(currentTick);
                _stepPhysiscsSimulationCommand.SetDeltaTime(stepDeltaTime).SetTick(currentTick).Execute();
                _updatePlayersLockOnWallStateCommand.SetTick(currentTick).Execute();
                _handleIfAnyPlayerChangedTeamFloorCommand.SetTick(currentTick).Execute();
                _handleIfStartMatchEligiblityChangedCommand.SetTick(currentTick).Execute();
                MoveToMatchStateIfCountdownEnded();
                RemoveOlderThanTickEventsPerClient(processPlayersInputsResult.HeighestProcessedTickPerClient);
                SendCurrentTickStateToAllClients(currentTick);
                _headLessQuitterController.QuitIfTimeOut();
            }
            catch (Exception e)
            {
                LogService.LogError("Got error! " + e);
                throw;
            }
        }
        
        private void MoveToMatchStateIfCountdownEnded()
        {
            if (!_startMatchWallController.DidFinishCountingDown)
            {
                return;
            }

            var dict = new Dictionary<long, EnterMatchPlayerData[]>();

            foreach (var clientNetworkData in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                var playersData = new EnterMatchPlayerData[clientNetworkData.Value.PlayerIds.Count];

                for (int i = 0; i < playersData.Length; i++)
                {
                    var playerState = _matchMakingDataService.SimulationState.GetPlayerById(clientNetworkData.Value.PlayerIds[i]);
                    playersData[i] = new EnterMatchPlayerData(playerState.Id, playerState.Name, playerState.TeamId);
                }
                dict.Add(clientNetworkData.Key, playersData);
            }
            var matchEnterData = new SimulationMatchEnterData(dict);
            _simulationStateMachine.ChangeToMatch(matchEnterData);
        }

        private ProcessPlayersInputsResult ProcessPackets(int processedTick)
        {
            _playerJoinPacketsHandler.ProcessPlayersJoined(processedTick);
            return _playerInputsPacketsHandler.ProcessInputs(processedTick);
        }

        private void RemoveOlderThanTickEventsPerClient(CapacityDict<long, int> heighestProcessedTickPerClient)
        {
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                var clientId = kvp.Key;

                if (heighestProcessedTickPerClient.TryGetValue(clientId, out int heighestProcessedTick))
                {
                    _netEventsDataService.RemoveAllEventsOlderThanTick(clientId, heighestProcessedTick);
                }
            }
        }

        private void SendCurrentTickStateToAllClients(int processedTick)
        {
            if (_matchMakingDataService.SimulationState.Players.Count == 0)
            {
                return;
            }

            var currentSimulationState = _matchMakingDataService.SimulationState;
            _fullTickPacket.Tick = processedTick;
            _fullTickPacket.CurrentSimulationState = currentSimulationState;
            
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                if (!kvp.Value.IsConnected)
                {
                    continue;
                }

                var clientId = kvp.Key;
                _fullTickPacket.BulletSpawnNetEvents = _netEventsDataService.BulletSpawnNetEventsPerClient[clientId];
                _fullTickPacket.PlayerJoinAcceptNetEvents = _netEventsDataService.MatchMakingPlayerJoinAcceptNetEventsPerClient[clientId];
                _fullTickPacket.BulletDestroyedNetEvents = _netEventsDataService.BulletDestroyedNetEventsPerClient[clientId];
                _fullTickPacket.PlayerSwitchTeamNetEvents = _netEventsDataService.PlayerSwitchTeamNetEventsPerClient[clientId];
                _fullTickPacket.StartMatchCountdownNetEvents = _netEventsDataService.StartMatchCountdownNetEventsPerClient[clientId];
                _fullTickPacket.StopMatchCountdownNetEvents = _netEventsDataService.StopMatchCountdownNetEventsPerClient[clientId];
                _fullTickPacket.StartMatchEligibleChangedNetEvents = _netEventsDataService.StartMatchEligibleChangedNetEventsPerClient[clientId];
                _fullTickPacket.PlayerLockOnTargetsChangedNetEvents = _netEventsDataService.PlayerLockOnTargetsChangedNetEventsPerClient[clientId];
                _fullTickPacket.PlayerLockedOnTargetHitNetEvents = _netEventsDataService.PlayerLockedOnTargetHitNetEventsPerClient[clientId];
                _networkManager.SendPacketToClientSerialized(clientId, PacketTypeS2C.MatchMakingFullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}