using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsHandlers
{
    public class PlayerInputsPacketsHandler : IPlayerInputsPacketsHandler
    {
        public PacketTypeC2S PacketType => PacketTypeC2S.PlayerInput;

        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;

        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;

        // private readonly Dictionary<int, Dictionary<int, PlayerInputPacketC2S>> _inputsByTick = new ();
        // Changed from int to ushort because playerId is defined as ushort in OnPlayerInputReceived
        private readonly CapacityDict<ushort, List<PlayerInputPacketC2S>> _inputsPerPlayer; //= new();
        private readonly CapacityDict<ushort, PlayerInputPacketC2S> _lastProcessedInputPerPlayer; //= new ();
        private readonly ConcurrentPool<PlayerInputPacketC2S> _playerInputPacketsPool;
        private readonly CapacityDict<ushort,PlayerInputPacketC2S> _cachedEarliestInputsPerPlayer;
        private readonly ConcurrentPool<List<PlayerInputPacketC2S>> _inputsListsPool;

        public PlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, IMatchNetEventsDataService matchNetEventsDataService, IPhysicsSimulator physicsSimulator)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _matchNetEventsDataService = matchNetEventsDataService;
            _physicsSimulator = physicsSimulator;
            _lastProcessedInputPerPlayer = new CapacityDict<ushort, PlayerInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _cachedEarliestInputsPerPlayer = new CapacityDict<ushort, PlayerInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerPlayer = new CapacityDict<ushort, List<PlayerInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            var inputPacketsSavedPerPlayer = networkConfig.MaxCap.PlayersInputsPackets / networkConfig.MaxCap.ConcurrentPlayers;
            _inputsListsPool = new ConcurrentPool<List<PlayerInputPacketC2S>>(() => new List<PlayerInputPacketC2S>(inputPacketsSavedPerPlayer), networkConfig.MaxCap.ConcurrentPlayers);
            _playerInputPacketsPool = new ConcurrentPool<PlayerInputPacketC2S>(() => new PlayerInputPacketC2S(), networkConfig.MaxCap.PlayersInputsPackets);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }
        
        public CapacityDict<ushort, PlayerInputPacketC2S> ProcessInputs(int processedTick)
        {
            var earliestInputPerPlayers = PopEarliestInputsOfEachPlayer();            
            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                ref var playerState = ref _matchDataService.SimulationState.GetPlayerByIndex(i);
                var playerId = playerState.Id;
                if (!earliestInputPerPlayers.ContainsKey(playerId))
                {
                    LogService.LogTopic($"Didn't find any last cached inputs for player {playerId}!", LogTopicType.ServerNetwork);
                    continue;
                }

                var playerInputPacket = earliestInputPerPlayers[playerId];
                UpdatePlayerDirection(playerInputPacket, ref playerState);
                UpdatePlayerShoot(processedTick, playerInputPacket.IsShootInputPressed, ref playerState);

                if (_lastProcessedInputPerPlayer.TryGetValue(playerId, out var lastPlayerInput))
                {
                    _playerInputPacketsPool.Return(lastPlayerInput);
                }
                _lastProcessedInputPerPlayer[playerId] = playerInputPacket;
            }
            
            return earliestInputPerPlayers;
        }
//         public Dictionary<ushort, PlayerInputPacketC2S> ProcessInputs(int processedTick)
//         {
//             var earliestInputPerPlayers = PopExceptLastInputsOfEachPlayer();//PopEarliestInputsOfEachPlayer();            
//             for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
//             {
//                 var player = _matchDataService.SimulationState.Players[i];
//                 var playerId = player.Id;
//                 if (!earliestInputPerPlayers.ContainsKey(playerId))
//                 {
// #if Logs
//                     LogService.LogTopic($"Didn't find any last cached inputs for player {playerId}!", LogTopicType.ServerNetwork);
// #endif
//                     continue;
//                 }
//
//                 var inputs = earliestInputPerPlayers[playerId];
//                 foreach (var playerInputPacket in inputs)
//                 {
//                     //var playerInputPacket = earliestInputPerPlayers[playerId];
//                     var playerModel = _matchDataService.GetPlayer(playerId);
//                     UpdatePlayerDirection(playerInputPacket, ref playerModel);
//                     UpdatePlayerShoot(processedTick, playerInputPacket.IsShootInputPressed, ref playerModel);
//                     _matchDataService.SetPlayer(playerId, playerModel);
//                     _cachedLastProcessedInput[playerId] = playerInputPacket;
//                     
//                 }
//             }
//             
//             //return earliestInputPerPlayers;
//             return null;
//         }

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
            LogService.LogTopic($"CreateBulletForPlayer {bullet.ToJson()}", LogTopicType.ServerNetwork);
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

        // private Dictionary<ushort, PlayerInputPacketC2S> PopLastInputsOfEachPlayer()
        // {
        //     var earliestInputsPerPlayer = new Dictionary<ushort, PlayerInputPacketC2S>();
        //
        //     for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
        //     {
        //         var playerState = _matchDataService.SimulationState.Players[i];
        //         var playerId = playerState.Id;
        //         PlayerInputPacketC2S earliestPlayerInput;
        //         if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs))
        //         {
        //             playerInputs.Sort();
        //             earliestPlayerInput = playerInputs.Last();
        //
        //             foreach (var playerInput in playerInputs)
        //             {
        //                 _playerInputPacketsPool.Return(playerInput);
        //             }
        //             playerInputs.Clear();
        //             if (playerInputs.Count == 0)
        //             {
        //                 _inputsPerPlayer.Remove(playerId);
        //             }
        //         }
        //         else
        //         {
        //             if (!TryGetCachedInputForPlayer(playerId, out earliestPlayerInput))
        //             {
        //                 continue;
        //             }
        //         }
        //         
        //         earliestInputsPerPlayer.Add(playerId, earliestPlayerInput);
        //     }
        //
        //     return earliestInputsPerPlayer;
        // }
        
        // private Dictionary<ushort, List<PlayerInputPacketC2S>> PopExceptLastInputsOfEachPlayer()
        // {
        //     var exceptLastInputsPerPlayer = new Dictionary<ushort, List<PlayerInputPacketC2S>>();
        //
        //     for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
        //     {
        //         var playerState = _matchDataService.SimulationState.Players[i];
        //         var playerId = playerState.Id;
        //         List<PlayerInputPacketC2S> exceptLastPlayerInputs =new List<PlayerInputPacketC2S>();
        //         if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs))
        //         {
        //             playerInputs.Sort();
        //             exceptLastPlayerInputs = new List<PlayerInputPacketC2S>();
        //             var count = playerInputs.Count;
        //             for (int j = 0; j < count; j++)
        //             {
        //                 exceptLastPlayerInputs.Add(playerInputs[0]);
        //                 playerInputs.RemoveAt(0);
        //             }
        //             
        //             if (count == 0)
        //             {
        //                 _inputsPerPlayer.Remove(playerId);
        //             }
        //         }
        //         else
        //         {
        //             if (!TryGetCachedInputForPlayer(playerId, out var lastCachedPlayerInput))
        //             {
        //                 continue;
        //             }
        //             exceptLastPlayerInputs.Add(lastCachedPlayerInput);
        //         }
        //
        //         exceptLastInputsPerPlayer.Add(playerId, exceptLastPlayerInputs);
        //     }
        //
        //     return exceptLastInputsPerPlayer;
        // }
        
        private CapacityDict<ushort, PlayerInputPacketC2S> PopEarliestInputsOfEachPlayer()
        {
            _cachedEarliestInputsPerPlayer.Clear();

            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
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
                        _inputsListsPool.Return(_inputsPerPlayer[playerId]);
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

                // if (earliestPlayerInput.IsShootInputPressed)
                // {
                //     var amountOfInputs = _inputsPerPlayer.ContainsKey(playerId) ? _inputsPerPlayer[playerId].Count : 0;
                //     string time = DateTime.Now.ToString("HH:mm:ss.fff");
                //     Debug.Log($"{time} Shoot processed!! earliestPlayerInput:{earliestPlayerInput.ToJson()}, {amountOfInputs}, {_inputsPerPlayer.ToJson()}");
                // }
                _cachedEarliestInputsPerPlayer.Add(playerId, earliestPlayerInput);
            }

            return _cachedEarliestInputsPerPlayer;
        }

        private bool TryGetCachedInputForPlayer(ushort playerId, out PlayerInputPacketC2S playerInputPacket)
        {
            return _lastProcessedInputPerPlayer.TryGetValue(playerId, out playerInputPacket);
        }
        
        public void OnPacketReceived(NetPacketReader reader, NetPeer peer)
        {
            var newPacket = _playerInputPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnPlayerInputReceived(newPacket, peer);
        }

        private void OnPlayerInputReceived(PlayerInputPacketC2S playerInputPacket, NetPeer peer)
        {
            var playerId = (ushort)peer.Tag;
            _inputsPerPlayer.TryAdd(playerId, _inputsListsPool.Get());
            _inputsPerPlayer[playerId].Add(playerInputPacket);
            
            if (playerInputPacket.IsShootInputPressed)
            {
                //string time = DateTime.Now.ToString("HH:mm:ss.fff");

      //          Debug.Log($"{time} Shoot Received!! playerInputPacket:{playerInputPacket.ToJson()}, {_inputsPerPlayer[playerId].Count}, {_inputsPerPlayer.ToJson()}");
            }
            LogService.LogTopic($"Input packet received from player id {playerId}, input: {playerInputPacket.ToJson()}, inputs per player: {_inputsPerPlayer.ToJson()}", LogTopicType.ServerNetwork);
        }
    }
}