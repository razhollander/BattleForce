using System;
using System.Threading;
using Box2D.NetStandard.Dynamics.Bodies;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager
{
    public class ServerNetworkTickProcessor : ITickProcessor
    {
        public int CurrentTick { get; private set; }

        private readonly NetworkConfig _networkConfig;
        private readonly IServerNetworkManager _networkManager;
        private readonly IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private readonly IMatchDataService _matchDataService;
        private readonly IPlayerBulletsTransformHandler _playerBulletsTransformHandler;
        private readonly IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IPlayersTransformHandler _playersTransformHandler;
        private readonly ICommandFactory _commandFactory;

        private TimerFixedThreaded _fixedTimer;
        private ProcessCachedCollisionsCommand _processCachedCollisionsCommand;
        private FullTickPacket _fullTickPacket;

        public ServerNetworkTickProcessor(NetworkConfig networkConfig, IServerNetworkManager networkManager,
            IPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchDataService matchDataService,
            IPlayerBulletsTransformHandler playerBulletsTransformHandler,
            IPlayerJoinPacketsHandler playerJoinPacketsHandler, IMatchNetEventsDataService matchNetEventsDataService, IPhysicsSimulator physicsSimulator,
            IPlayersTransformHandler playersTransformHandler, ICommandFactory commandFactory)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchDataService = matchDataService;
            _playerBulletsTransformHandler = playerBulletsTransformHandler;
            _playerJoinPacketsHandler = playerJoinPacketsHandler;
            _matchNetEventsDataService = matchNetEventsDataService;
            _physicsSimulator = physicsSimulator;
            _playersTransformHandler = playersTransformHandler;
            _commandFactory = commandFactory;
        }

        public void InitEntryPoint()
        {
            StartTick();
            _fullTickPacket = new FullTickPacket(_networkConfig.MaxCap);
            _processCachedCollisionsCommand = _commandFactory.CreateCommandVoid<ProcessCachedCollisionsCommand>();
        }

        private void StartTick()
        {
            CurrentTick = 0;
            var cancellationTokenSource = new CancellationTokenSource();
            _fixedTimer = new TimerFixedThreaded(_networkConfig.TicksPerSeconds, OnTick);
            _fixedTimer.Start(cancellationTokenSource/*_stateMachineService.CurrentState().CancellationTokenSource*/);
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
                CurrentTick++;
                // string time = DateTime.Now.ToString("HH:mm:ss.fff");

                // Debug.Log($"{time} OnTick!");

                var processedTick = CurrentTick - _networkConfig.ServerTicksBuffer;
                _networkManager.PollEvents();
                var processPlayersInputsResult = ProcessPackets(processedTick);
                ApplyMatchModelToPhysicsSimulation();
                _physicsSimulator.Step(_networkConfig.DeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
                _processCachedCollisionsCommand.SetProcessedTick(processedTick).Execute();
                ApplyPhysicsSimulationToMatchModel();
                 RemoveOlderThanTickEventsPerPlayer(processPlayersInputsResult.HeighestProcessedTickPerPlayer);
                 SendCurrentTickStateToAllClients(processedTick);
                 //_matchDataService.CopySimulationStateIntoPrevious();
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
                ref var playerState = ref _matchDataService.SimulationState.Players.GetByIndex(i);
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
                    _matchNetEventsDataService.RemoveAllEventsOlderThanTick(playerId, tickOfPlayer);
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
                _fullTickPacket.BulletSpawnNetEvents = _matchNetEventsDataService.BulletSpawnNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerJoinAcceptNetEvents = _matchNetEventsDataService.JoinAcceptNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerTakeDamageNetEvents = _matchNetEventsDataService.PlayerTakeDamageNetEventsPerPlayer[playerId];
                _fullTickPacket.BulletDestroyedNetEvents = _matchNetEventsDataService.BulletDestroyedNetEventsPerPlayer[playerId];
                _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.FullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}