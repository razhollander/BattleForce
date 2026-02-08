using System;
using System.Diagnostics;
using System.Threading;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
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

        private StepTimersCommand _stepTimersCommand;
        private StepPhysiscsSimulationCommand _stepPhysiscsSimulationCommand;
        private MatchMakingFullTickPacketS2C _fullTickPacket;
        private Stopwatch _sw;
        private long _last;

        public ServerMatchMakingNetworkTickProcessor(NetworkConfig networkConfig, IServerNetworkManager networkManager,
            IPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchMakingDataService matchMakingDataService,
            IPlayerJoinPacketsHandler playerJoinPacketsHandler, INetEventsDataService iNetEventsDataService,
            ICommandFactory commandFactory, ITickService tickService, IStartMatchWallController startMatchWallController, ISimulationStateMachine simulationStateMachine,
            IHeadLessQuitterController headLessQuitterController)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchMakingDataService = matchMakingDataService;
            _playerJoinPacketsHandler = playerJoinPacketsHandler;
            _netEventsDataService = iNetEventsDataService;
            _commandFactory = commandFactory;
            _tickService = tickService;
            _startMatchWallController = startMatchWallController;
            _simulationStateMachine = simulationStateMachine;
            _headLessQuitterController = headLessQuitterController;
        }

        public void InitEntryPoint()
        {
            _fullTickPacket = new MatchMakingFullTickPacketS2C();
            _stepTimersCommand = _commandFactory.CreateCommandVoid<StepTimersCommand>();
            _stepPhysiscsSimulationCommand = _commandFactory.CreateCommandVoid<StepPhysiscsSimulationCommand>();
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
                MoveToMatchStateIfCountdownEnded();
                RemoveOlderThanTickEventsPerPlayer(processPlayersInputsResult.HeighestProcessedTickPerPlayer);
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

            var playersData = new EnterMatchPlayerData[_matchMakingDataService.SimulationState.Players.Count];

            for (int i = 0; i < _matchMakingDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchMakingDataService.SimulationState.Players.GetByIndex(i);
                playersData[i] = new EnterMatchPlayerData(playerState.Id, playerState.Name, playerState.TeamId);
            }

            var matchEnterData = new SimulationMatchEnterData(playersData);
            _simulationStateMachine.ChangeToMatch(matchEnterData);
        }

        private ProcessPlayersInputsResult ProcessPackets(int processedTick)
        {
            _playerJoinPacketsHandler.ProcessPlayersJoined(processedTick);
            return _playerInputsPacketsHandler.ProcessInputs(processedTick);
        }

        private void RemoveOlderThanTickEventsPerPlayer(CapacityDict<ushort, int> heighestProcessedTickPerPlayer)
        {
            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())        
            {
                var playerId = playerState.Id;

                if (heighestProcessedTickPerPlayer.TryGetValue(playerId, out int heighestProcessedTick))
                {
                    _netEventsDataService.RemoveAllEventsOlderThanTick(playerId, heighestProcessedTick);
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
            //_fullTickPacket.PreviousSimulationState = _matchDataService.PreviousSimulationState;
            foreach (var playerState in currentSimulationState.Players.AsSpan())
            {
                var playerId = playerState.Id;
                _fullTickPacket.BulletSpawnNetEvents = _netEventsDataService.BulletSpawnNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerJoinAcceptNetEvents = _netEventsDataService.MatchMakingPlayerJoinAcceptNetEventsPerPlayer[playerId];
                _fullTickPacket.BulletDestroyedNetEvents = _netEventsDataService.BulletDestroyedNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerSwitchTeamNetEvents = _netEventsDataService.PlayerSwitchTeamNetEventsPerPlayer[playerId];
                _fullTickPacket.StartMatchCountdownNetEvents = _netEventsDataService.StartMatchCountdownNetEventsPerPlayer[playerId];
                _fullTickPacket.StopMatchCountdownNetEvents = _netEventsDataService.StopMatchCountdownNetEventsPerPlayer[playerId];
                _fullTickPacket.StartMatchEligibleChangedNetEvents = _netEventsDataService.StartMatchEligibleChangedNetEventsPerPlayer[playerId];
                _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.MatchMakingFullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}