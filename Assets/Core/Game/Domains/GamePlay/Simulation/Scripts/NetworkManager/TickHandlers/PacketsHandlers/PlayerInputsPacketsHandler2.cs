using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsHandlers
{
    public class PlayerInputsPacketsHandler : IPlayerInputsPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;

        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;

        // private readonly Dictionary<int, Dictionary<int, PlayerInputPacketC2S>> _inputsByTick = new ();
        // Changed from int to ushort because playerId is defined as ushort in OnPlayerInputReceived
        private readonly Dictionary<ushort, List<PlayerInputPacketC2S>> _inputsPerPlayer = new();
        private readonly Dictionary<ushort,PlayerInputPacketC2S> _cachedLastProcessedInput = new ();

        public PlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, IMatchNetEventsDataService matchNetEventsDataService, IPhysicsSimulator physicsSimulator)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _matchNetEventsDataService = matchNetEventsDataService;
            _physicsSimulator = physicsSimulator;
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<PlayerInputPacketC2S>(OnPlayerInputReceived);
        }

        public void InitExitPoint()
        {
            _networkManager.RemoveSubscription<PlayerInputPacketC2S>();
        }

        public Dictionary<ushort, PlayerInputPacketC2S> ProcessInputs(int processedTick)
        {
            var earliestInputPerPlayers = /*PopLastInputsOfEachPlayer();*/PopEarliestInputsOfEachPlayer();            
            for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var player = _matchDataService.SimulationState.Players[i];
                var playerId = player.Id;
                if (!earliestInputPerPlayers.ContainsKey(playerId))
                {
#if Logs
                    LogService.LogTopic($"Didn't find any last cached inputs for player {playerId}!", LogTopicType.ServerNetwork);
#endif
                    continue;
                }

                var playerInputPacket = earliestInputPerPlayers[playerId];
                var playerModel = _matchDataService.GetPlayer(playerId);
                UpdatePlayerDirection(playerInputPacket, ref playerModel);
                UpdatePlayerShoot(processedTick, playerInputPacket.IsShootInputPressed, ref playerModel);
                _matchDataService.SetPlayer(playerId, playerModel);
                _cachedLastProcessedInput[playerId] = playerInputPacket;
            }
            
            return earliestInputPerPlayers;
        }

        private void UpdatePlayerShoot(int processedTick, bool isShootInputPressed, ref PlayerStateS2C playerModel)
        {
            var shootState = playerModel.Spaceship.Shoot;
            var isCurrentlyOnCooldown = shootState.CooldownSecondsLeft < shootState.MaxCooldown;
            if (isCurrentlyOnCooldown)
            {
                shootState.CooldownSecondsLeft -= _networkConfig.DeltaTime;
            }

            if (shootState.CooldownSecondsLeft < 0)
            {
                shootState.CooldownSecondsLeft = shootState.MaxCooldown;
            }

            var shouldShoot = isShootInputPressed && shootState.CooldownSecondsLeft == shootState.MaxCooldown;
            if (shouldShoot)
            {
                shootState.CooldownSecondsLeft -= _networkConfig.DeltaTime;
                CreateBulletForPlayer(processedTick, playerModel);
            }

            playerModel.Spaceship.Shoot = shootState;
        }

        private void CreateBulletForPlayer(int processedTick, PlayerStateS2C playerModel)
        {
            var bullet = _matchDataService.AddBullet(playerModel.Id, playerModel.Spaceship.Transform.GetHeadPosition(),
                playerModel.Spaceship.Transform.Direction, _gamePlayConfig.PlayerBullet.MoveSpeed, _gamePlayConfig.PlayerBullet.Radius);
            _matchNetEventsDataService.AddBulletSpawnNetEvent(processedTick, bullet.Id, bullet.BelongToPlayerId, bullet.Position, bullet.Radius);
            _physicsSimulator.AddPlayerBullet(bullet.Id, playerModel.TeamId, bullet.Position, bullet.Velocity, bullet.Radius);
#if Logs
            LogService.LogTopic($"CreateBulletForPlayer {bullet.ToJson()}", LogTopicType.ServerNetwork);
#endif
        }

        private void UpdatePlayerDirection(PlayerInputPacketC2S playerInputPacket, ref PlayerStateS2C playerModel)
        {
            var rotationDelta = _gamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (playerInputPacket.IsMoveLeftInputPressed.ToInt() -
                 playerInputPacket.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerModel.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerModel.Spaceship.Transform.Direction = rotatedVector;
            playerModel.Spaceship.Transform.Velocity = playerModel.Spaceship.Transform.Direction * _gamePlayConfig.PlayerSpaceship.MovementSpeed;
        }

        private Dictionary<ushort, PlayerInputPacketC2S> PopLastInputsOfEachPlayer()
        {
            var earliestInputsPerPlayer = new Dictionary<ushort, PlayerInputPacketC2S>();

            for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var playerState = _matchDataService.SimulationState.Players[i];
                var playerId = playerState.Id;
                PlayerInputPacketC2S earliestPlayerInput;
                if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs))
                {
                    playerInputs.Sort();
                    earliestPlayerInput = playerInputs.Last();
                    playerInputs.Clear();
                    if (playerInputs.Count == 0)
                    {
                        _inputsPerPlayer.Remove(playerId);
                    }
                }
                else
                {
                    if (!TryGetCachedInputForPlayer(playerId, out earliestPlayerInput))
                    {
                        continue;
                    }
                }

                if (earliestPlayerInput.IsShootInputPressed)
                {
                    var amountOfInputs = _inputsPerPlayer.ContainsKey(playerId) ? _inputsPerPlayer[playerId].Count : 0;
                    string time = DateTime.Now.ToString("HH:mm:ss.fff");
                    Debug.Log($"{time} Shoot processed!! earliestPlayerInput:{earliestPlayerInput.ToJson()}, {amountOfInputs}, {_inputsPerPlayer.ToJson()}");
                }
                earliestInputsPerPlayer.Add(playerId, earliestPlayerInput);
            }

            return earliestInputsPerPlayer;
        }
        
        private Dictionary<ushort, PlayerInputPacketC2S> PopEarliestInputsOfEachPlayer()
        {
            var earliestInputsPerPlayer = new Dictionary<ushort, PlayerInputPacketC2S>();

            for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var playerState = _matchDataService.SimulationState.Players[i];
                var playerId = playerState.Id;
                PlayerInputPacketC2S earliestPlayerInput;
                if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs))
                {
                    playerInputs.Sort();
                    earliestPlayerInput = playerInputs[0];
                    playerInputs.RemoveAt(0);
                    if (playerInputs.Count == 0)
                    {
                        _inputsPerPlayer.Remove(playerId);
                    }
                }
                else
                {
                    if (!TryGetCachedInputForPlayer(playerId, out earliestPlayerInput))
                    {
                        continue;
                    }
                }

                if (earliestPlayerInput.IsShootInputPressed)
                {
                    var amountOfInputs = _inputsPerPlayer.ContainsKey(playerId) ? _inputsPerPlayer[playerId].Count : 0;
                    string time = DateTime.Now.ToString("HH:mm:ss.fff");
                    Debug.Log($"{time} Shoot processed!! earliestPlayerInput:{earliestPlayerInput.ToJson()}, {amountOfInputs}, {_inputsPerPlayer.ToJson()}");
                }
                earliestInputsPerPlayer.Add(playerId, earliestPlayerInput);
            }

            return earliestInputsPerPlayer;
        }

        private bool TryGetCachedInputForPlayer(ushort playerId, out PlayerInputPacketC2S playerInputPacket)
        {
            return _cachedLastProcessedInput.TryGetValue(playerId, out playerInputPacket);
        }

        private void OnPlayerInputReceived(PlayerInputPacketC2S playerInputPacket, NetPeer peer)
        {
            var playerId = (ushort)peer.Tag;
            _inputsPerPlayer.TryAdd(playerId, new List<PlayerInputPacketC2S>());
            _inputsPerPlayer[playerId].Add(playerInputPacket);
            
            if (playerInputPacket.IsShootInputPressed)
            {
                //string time = DateTime.Now.ToString("HH:mm:ss.fff");

      //          Debug.Log($"{time} Shoot Received!! playerInputPacket:{playerInputPacket.ToJson()}, {_inputsPerPlayer[playerId].Count}, {_inputsPerPlayer.ToJson()}");
            }
#if Logs
            LogService.LogTopic($"Input packet received from player id {playerId}, input: {playerInputPacket.ToJson()}, inputs per player: {_inputsPerPlayer.ToJson()}", LogTopicType.ServerNetwork);
#endif
        }
    }
}