using System;
using System.Threading;
using Box2D.NetStandard.Dynamics.Bodies;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
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

        private TimerFixedThreaded _fixedTimer;

        public ServerNetworkTickProcessor(NetworkConfig networkConfig, IServerNetworkManager networkManager,
            IPlayerInputsPacketsHandler playerInputsPacketsHandler, IMatchDataService matchDataService,
            IPlayerBulletsTransformHandler playerBulletsTransformHandler,
            IPlayerJoinPacketsHandler playerJoinPacketsHandler, IMatchNetEventsDataService matchNetEventsDataService, IPhysicsSimulator physicsSimulator,
            IPlayersTransformHandler playersTransformHandler)
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
        }

        public void InitEntryPoint()
        {
            StartTick();
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
                
                var processedTick = CurrentTick - _networkConfig.ServerTicksBuffer;
                _networkManager.PollEvents();
                ProcessPackets(processedTick);
                ApplyMatchModelToPhysicsSimulation();
                _physicsSimulator.Step(_networkConfig.DeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
                ProcessCollisions();
                ApplyPhysicsSimulationToMatchModel();
                RemoveOlderThanTickEventsPerPlayer(processedTick);
                SendCurrentTickStateToAllClients(processedTick);
                _matchDataService.CopySimulationStateIntoPrevious();
                //var inputsPerPlayerForCurrentTick = _serverPlayersInputListener.GetSortedInputsPerPlayerForTick(CurrentTick); 
            }
            catch (Exception e)
            {
                LogService.LogError("Got error! " + e);
                throw;
            }
        }

        private void ApplyPhysicsSimulationToMatchModel()
        {
            for (int i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var playerModel = _matchDataService.SimulationState.Players[i];
                playerModel.Spaceship.Transform.Position = _physicsSimulator.GetPlayer(playerModel.Id).Position;
                _matchDataService.SetPlayer(playerModel.Id, playerModel);
            }

            foreach (int usedIndex in _matchDataService.SimulationState.Bullets.UsedIndices())
            {
                var bulletModel = _matchDataService.SimulationState.Bullets[usedIndex];
                bulletModel.Position = _physicsSimulator.GetBullet(bulletModel.Id).Position;
                _matchDataService.SetBullet(bulletModel.Id, bulletModel);
            }
        }

        private void ApplyMatchModelToPhysicsSimulation()
        {
            _physicsSimulator.CopyDataToSimulation(_matchDataService.SimulationState);
        }

        private void ProcessPackets(int processedTick)
        {
            _playerJoinPacketsHandler.ProcessPlayersJoined(processedTick);
            _playerInputsPacketsHandler.ProcessInputs(processedTick);
        }

        private void ProcessCollisions()
        {
            var cachedCollisions = _physicsSimulator.GetCachedCollisions();

            foreach (var collisionEvent in cachedCollisions)
            {
                if (collisionEvent.Type != EventType.Begin)
                {
                    continue;
                }

                var objectA = (PhysicsBodyData) collisionEvent.FixtureA.Body.UserData;
                var objectB = (PhysicsBodyData) collisionEvent.FixtureB.Body.UserData;
                bool isPlayerToWallCollision = objectA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship && objectB.PhysicsBodyType == PhysicsBodyType.Wall;
                bool isWallToPlayerCollision = objectA.PhysicsBodyType == PhysicsBodyType.Wall && objectB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship;
                var isCollision = isPlayerToWallCollision || isWallToPlayerCollision;
                PlayerStateS2C playerModel = default;
                Body playerBody = default;

                if (isPlayerToWallCollision)
                {
                    playerModel = _matchDataService.GetPlayer(objectA.Id);
                }
                else if (isWallToPlayerCollision)
                {
                    playerModel = _matchDataService.GetPlayer(objectB.Id);
                }

                if (!isCollision)
                {
                    continue;
                }

                var relativeVelocity = playerModel.Spaceship.Transform.Velocity;
                collisionEvent.Contact.GetWorldManifold(out var worldManifold);
                var collisionNormal = worldManifold.normal;
                var reflectedVelocity = relativeVelocity.ReflectFromWall(collisionNormal);
                playerModel.Spaceship.Transform.Velocity = reflectedVelocity;
                //Debug.Log($"new pos {_physicsSimulator.GetPlayer(playerModel.Id).Position}, prev pos: {playerModel.Spaceship.Transform.Position} ");
                playerModel.Spaceship.Transform.Direction = reflectedVelocity.Length() > 0
                    ? System.Numerics.Vector2.Normalize(reflectedVelocity)
                    : System.Numerics.Vector2.Zero;

                Debug.Log("Collision!");
                _matchDataService.SetPlayer(playerModel.Id, playerModel);
            }

            _physicsSimulator.ClearCachedCollisions();
        }

        private void RemoveOlderThanTickEventsPerPlayer(int processedTick)
        {
            for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var player = _matchDataService.SimulationState.Players[i];
                var playerId = player.Id;
                _matchNetEventsDataService.RemoveAllEventsOlderThanTick(playerId, processedTick);
            }
        }

        private void SendCurrentTickStateToAllClients(int processedTick)
        {
            if (_matchDataService.SimulationState.PlayersCount == 0)
            {
                return;
            }

            var simulationState = _matchDataService.SimulationState;
            var packet = new FullTickPacket(processedTick, _matchDataService.PreviousSimulationState, simulationState, null, null);
            for (var i = 0; i < simulationState.PlayersCount; i++)
            {
                var playerState = simulationState.Players[i];
                var playerId = playerState.Id;
                packet.BulletSpawnNetEvents = _matchNetEventsDataService.BulletSpawnNetEventsPerPlayer[playerId];
                packet.PlayerJoinAcceptNetEvents = _matchNetEventsDataService.JoinAcceptNetEventsPerPlayer[playerId];
                _networkManager.SendPacketToPlayerSerialized(playerId, PacketTypeS2C.FullTick, packet,
                    DeliveryMethod.Unreliable);
            }
        }
    }
}