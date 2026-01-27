using System;
using System.Diagnostics;
using System.Threading;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
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
        private readonly IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private readonly IMatchDataService _matchDataService;
        private readonly IPlayeRejoinPacketsHandler _playeRejoinPacketsHandler;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly ICommandFactory _commandFactory;
        private readonly ITickCounterService _tickCounterService;
        private readonly IPlaybackRecorderService _playbackRecorderService;

        private TimerFixedThreaded2 _fixedTimer;
        private ProcessCachedCollisionsCommand _processCachedCollisionsCommand;
        private TryDamagePlayersInLavaCommand _tryDamagePlayersInLavaCommand;
        private TrySpawnPowerUpBallsCommand _trySpawnPowerUpBallsCommand;
        private StepTimersCommand _stepTimersCommand;
        private MatchFullTickPacket _fullTickPacket;
        //private TimerFixedThreaded2 _pollEventsFixedTimer;
        private Stopwatch _sw;
        private long _last;

        public ServerMatchNetworkTickProcessor(NetworkConfig networkConfig, IServerNetworkManager networkManager,
            IPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchDataService matchDataService,
            IPlayeRejoinPacketsHandler iPlayeRejoinPacketsHandler, INetEventsDataService iNetEventsDataService, IPhysicsSimulator physicsSimulator,
            ICommandFactory commandFactory, ITickCounterService tickCounterService, IPlaybackRecorderService playbackRecorderService)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchDataService = matchDataService;
            _playeRejoinPacketsHandler = iPlayeRejoinPacketsHandler;
            _netEventsDataService = iNetEventsDataService;
            _physicsSimulator = physicsSimulator;
            _commandFactory = commandFactory;
            _tickCounterService = tickCounterService;
            _playbackRecorderService = playbackRecorderService;
        }

        public void InitEntryPoint()
        {
            StartTick();
            _fullTickPacket = new MatchFullTickPacket();
            _processCachedCollisionsCommand = _commandFactory.CreateCommandVoid<ProcessCachedCollisionsCommand>();
            _tryDamagePlayersInLavaCommand = _commandFactory.CreateCommandVoid<TryDamagePlayersInLavaCommand>();
            _trySpawnPowerUpBallsCommand = _commandFactory.CreateCommandVoid<TrySpawnPowerUpBallsCommand>();
            _stepTimersCommand = _commandFactory.CreateCommandVoid<StepTimersCommand>();
        }

        private void StartTick()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _fixedTimer = new TimerFixedThreaded2("BattleFroce Thread", _networkConfig.TicksPerSeconds, OnTick);
            // _pollEventsFixedTimer = new TimerFixedThreaded2("Poll Events Thread", -1, PollEvents);
            // _pollEventsFixedTimer.Start(cancellationTokenSource);
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
                //_networkManager.SetServerTick(CurrentTick); // Update tick for playback recording/reading
                var stepDeltaTime = _networkConfig.DeltaTime;
                _stepTimersCommand.SetStepDeltaTime(stepDeltaTime).Execute();
                _tickCounterService.IncrementTick();;
                var processedTick = _tickCounterService.CurrentTick - _networkConfig.ServerTicksBuffer;
                var processPlayersInputsResult = ProcessPackets(processedTick);
                _trySpawnPowerUpBallsCommand.SetProcessedTick(processedTick).Execute();
                
                ApplyMatchModelToPhysicsSimulation();
                _physicsSimulator.Step(stepDeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
                ApplyPhysicsSimulationToMatchModel();
                
                _processCachedCollisionsCommand.SetProcessedTick(processedTick).Execute();
                _tryDamagePlayersInLavaCommand.SetProcessedTick(processedTick).Execute();
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

            for (int i = 0; i < _matchDataService.SimulationState.PowerUpBalls.Count; i++)
            {
                ref var powerUpBallState = ref _matchDataService.SimulationState.PowerUpBalls.GetByIndex(i);
                powerUpBallState.Position = _physicsSimulator.GetPowerUpBall(powerUpBallState.Id).Position;
            }
        }

        private void ApplyMatchModelToPhysicsSimulation()
        {
            _physicsSimulator.CopyDataToSimulation(_matchDataService.SimulationState);
        }

        private ProcessPlayersInputsResult ProcessPackets(int processedTick)
        {
            _playeRejoinPacketsHandler.ProcessPlayersRejoined(processedTick);
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
                _fullTickPacket.PlayerJoinAcceptNetEvents = _netEventsDataService.PlayerRejoinAcceptNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerTakeDamageNetEvents = _netEventsDataService.PlayerTakeDamageNetEventsPerPlayer[playerId];
                _fullTickPacket.BulletDestroyedNetEvents = _netEventsDataService.BulletDestroyedNetEventsPerPlayer[playerId];
                _fullTickPacket.PlayerSwapNetEvents = _netEventsDataService.PlayerSwapNetEventsPerPlayer[playerId];
                _fullTickPacket.TalentCardObtainedNetEvents = _netEventsDataService.TalentCardObtainedNetEventsPerPlayer[playerId];
                _fullTickPacket.TalentCardHitNetEvents = _netEventsDataService.TalentCardHitNetEventsPerPlayer[playerId];
                _fullTickPacket.PowerUpSpawnedNetEvents = _netEventsDataService.PowerUpBallSpawnedNetEventsPerPlayer[playerId];
                _fullTickPacket.PowerUpObtainedNetEvents = _netEventsDataService.PowerUpBallObtainedNetEventsPerPlayer[playerId];
                _fullTickPacket.StartMatchNetEvents = _netEventsDataService.StartMatchNetEventsPerPlayer[playerId];
                _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.MatchFullTick, _fullTickPacket,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}