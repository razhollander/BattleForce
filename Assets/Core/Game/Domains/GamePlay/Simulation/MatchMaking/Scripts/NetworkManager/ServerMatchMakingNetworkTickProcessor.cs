using System;
using System.Diagnostics;
using System.Threading;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
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
        private readonly IMatchMakingDataService _matchDataService;
        private readonly IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly ICommandFactory _commandFactory;
        private readonly ITickCounterService _tickCounterService;

        private TimerFixedThreaded2 _fixedTimer;
        private MatchMakingProcessCachedCollisionsCommand _processCachedCollisionsCommand;
        private StepTimersCommand _stepTimersCommand;
        private MatchMakingFullTickPacket _fullTickPacket;
        //private TimerFixedThreaded2 _pollEventsFixedTimer;
        private Stopwatch _sw;
        private long _last;

        public ServerMatchMakingNetworkTickProcessor(NetworkConfig networkConfig, IServerNetworkManager networkManager,
            IPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchMakingDataService matchDataService,
            IPlayerJoinPacketsHandler playerJoinPacketsHandler, INetEventsDataService iNetEventsDataService, IPhysicsSimulator physicsSimulator,
            ICommandFactory commandFactory, ITickCounterService tickCounterService)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchDataService = matchDataService;
            _playerJoinPacketsHandler = playerJoinPacketsHandler;
            _netEventsDataService = iNetEventsDataService;
            _physicsSimulator = physicsSimulator;
            _commandFactory = commandFactory;
            _tickCounterService = tickCounterService;
        }

        public void InitEntryPoint()
        {
            StartTick();
            _fullTickPacket = new MatchMakingFullTickPacket();
            _processCachedCollisionsCommand = _commandFactory.CreateCommandVoid<MatchMakingProcessCachedCollisionsCommand>();
            _stepTimersCommand = _commandFactory.CreateCommandVoid<StepTimersCommand>();
        }

        private void StartTick()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _fixedTimer = new TimerFixedThreaded2("BattleFroce MatchMaking Thread", _networkConfig.TicksPerSeconds, OnTick);
            _fixedTimer.Start(cancellationTokenSource);
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
            StopTick();
        }

        private void StopTick()
        {
            _fixedTimer.Stop();
        }
      
        private void OnTick()
        {
            try
            {
                _networkManager.PollEvents();
                var stepDeltaTime = _networkConfig.DeltaTime;
                _stepTimersCommand.SetStepDeltaTime(stepDeltaTime).Execute();
                _tickCounterService.IncrementTick();;
                var processedTick = _tickCounterService.CurrentTick - _networkConfig.ServerTicksBuffer;
                var processPlayersInputsResult = ProcessPackets(processedTick);
                
                ApplyMatchModelToPhysicsSimulation();
                _physicsSimulator.Step(stepDeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
                ApplyPhysicsSimulationToMatchModel();
                
                _processCachedCollisionsCommand.SetProcessedTick(processedTick).Execute();
                RemoveOlderThanTickEventsPerPlayer(processPlayersInputsResult.HeighestProcessedTickPerPlayer);
                SendCurrentTickStateToAllClients(processedTick);
            }
            catch (Exception e)
            {
                LogService.LogError("Got error! " + e);
                throw;
            }
        }

        private void ApplyPhysicsSimulationToMatchModel()
        {
            for (int i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.Players.GetByIndex(i);
                playerState.Spaceship.Transform.Position = _physicsSimulator.GetPlayer(playerState.Id).Position;
            }

            for (int i = 0; i < _matchDataService.SimulationState.Bullets.Count; i++)
            {
                ref var bulletState = ref _matchDataService.SimulationState.Bullets.GetByIndex(i);
                bulletState.Position = _physicsSimulator.GetBullet(bulletState.Id).Position;
            }
        }

        private void ApplyMatchModelToPhysicsSimulation()
        {
            _physicsSimulator.CopyDataToSimulation(_matchDataService.SimulationState);
        }

        private ProcessPlayersInputsResult ProcessPackets(int processedTick)
        {
            _playerJoinPacketsHandler.ProcessPlayersJoined(processedTick);
            return _playerInputsPacketsHandler.ProcessInputs(processedTick);
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
                var playerId = playerState.Id;
                _fullTickPacket.BulletSpawnNetEvents = _netEventsDataService.BulletSpawnNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerJoinAcceptNetEvents = _netEventsDataService.MatchMakingPlayerJoinAcceptNetEventsPerPlayer[playerId];
                _fullTickPacket.BulletDestroyedNetEvents = _netEventsDataService.BulletDestroyedNetEventsPerPlayer[playerId];
                _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.MatchMakingFullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}