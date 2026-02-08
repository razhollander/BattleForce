using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public class MatchMakingPlayerInputsPacketsHandler : IPlayerInputsPacketsHandler, IGUIUpdatable
    {
        public PacketTypeC2S PacketType => PacketTypeC2S.MatchMakingPlayerInput;

        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchMakingDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;

        private readonly CapacityDict<ushort, FixedUnorderedList<MatchMakingPlayerInputPacketC2S>> _inputsPerPlayer;
        private readonly CapacityDict<ushort, MatchMakingPlayerInputPacketC2S> _lastProcessedInputPerPlayer;
        private readonly ConcurrentPool<MatchMakingPlayerInputPacketC2S> _playerInputPacketsPool;
        private readonly ConcurrentPool<FixedUnorderedList<MatchMakingPlayerInputPacketC2S>> _inputsListsPool;
        private readonly ProcessPlayersInputsResult _cachedProcessPlayersInputsResult;

        public MatchMakingPlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchMakingDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, INetEventsDataService iNetEventsDataService, IPhysicsSimulator physicsSimulator, IUpdateSubscriptionService updateSubscriptionService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _netEventsDataService = iNetEventsDataService;
            _physicsSimulator = physicsSimulator;
            _updateSubscriptionService = updateSubscriptionService;
            _cachedProcessPlayersInputsResult = new ProcessPlayersInputsResult(networkConfig.MaxCap.ConcurrentPlayers);
            _lastProcessedInputPerPlayer = new CapacityDict<ushort, MatchMakingPlayerInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<MatchMakingPlayerInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            var inputPacketsSavedPerPlayer = networkConfig.MaxCap.PlayersInputsPackets / networkConfig.MaxCap.ConcurrentPlayers;
            _inputsListsPool = new ConcurrentPool<FixedUnorderedList<MatchMakingPlayerInputPacketC2S>>(() => new FixedUnorderedList<MatchMakingPlayerInputPacketC2S>(inputPacketsSavedPerPlayer), networkConfig.MaxCap.ConcurrentPlayers);
            _playerInputPacketsPool = new ConcurrentPool<MatchMakingPlayerInputPacketC2S>(() => new MatchMakingPlayerInputPacketC2S(), networkConfig.MaxCap.ConcurrentInputsProcessed);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
            _updateSubscriptionService.RegisterGuiUpdatable(this);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
            _updateSubscriptionService.UnregisterGuiUpdatable(this);
        }
        
        public ProcessPlayersInputsResult ProcessInputs(int processedTick)
        {
            _cachedProcessPlayersInputsResult.Clear();
            LeaveLatestPacketsForBuffer(_networkConfig.ServerPlayerInputPacketsBuffer);
            _cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer = GetHeighestProcessedTickFromServerPerPlayer();
            _cachedProcessPlayersInputsResult.EarliestInputsPerPlayer = ProcessEarliestInputPerPlayers(processedTick);
            return _cachedProcessPlayersInputsResult;
        }
        
        private void LeaveLatestPacketsForBuffer(int bufferAmount)
        {
            foreach (var kvp in _inputsPerPlayer)
            {
                var inputsOfPlayer = kvp.Value;
                var amountOfPacketsToRemove = inputsOfPlayer.Count - bufferAmount;
                var doesHaveLessPacketsThanBuffer = amountOfPacketsToRemove <= 0;
                if (doesHaveLessPacketsThanBuffer)
                {
                    continue;
                }

                RemoveAmountOfEarliestInputs(inputsOfPlayer, amountOfPacketsToRemove);
            }
        }

        private void RemoveAmountOfEarliestInputs(FixedUnorderedList<MatchMakingPlayerInputPacketC2S> inputsOfPlayer, int amountOfPacketsToRemove)
        {
            inputsOfPlayer.Sort();
            inputsOfPlayer.RemoveRange(0, amountOfPacketsToRemove);
        }

        private CapacityDict<ushort, MatchMakingPlayerInputPacketC2S> ProcessEarliestInputPerPlayers(int processedTick)
        {
            var earliestInputPerPlayers = PopEarliestInputsOfEachPlayer();

            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.GetPlayerByIndex(i);
                var playerId = playerState.Id;
                if (!earliestInputPerPlayers.TryGetValue(playerId, out var playerInputPacket))
                {
                    LogService.LogTopic($"Didn't find any last cached inputs for player {playerId}!", LogTopicType.ServerNetwork);
                    continue;
                }

                UpdatePlayerDirection(playerInputPacket, playerState);
                UpdatePlayerShoot(processedTick, playerInputPacket.IsShootInputPressed, playerState);

                if (_lastProcessedInputPerPlayer.TryGetValue(playerId, out var lastPlayerInput))
                {
                    _playerInputPacketsPool.Return(lastPlayerInput);
                }
                _lastProcessedInputPerPlayer[playerId] = playerInputPacket;
            }

            return earliestInputPerPlayers;
        }
        

        private CapacityDict<ushort, int> GetHeighestProcessedTickFromServerPerPlayer()
        {
            foreach (var inputsOfPlayer in _inputsPerPlayer)
            {
                var didReceiveAnyInputsFromPlayer = inputsOfPlayer.Value.Count > 0;
                if (didReceiveAnyInputsFromPlayer)
                {
                    var heighetsTick = GetMaxHighestProcessedTickFromServer(inputsOfPlayer.Value);
                    _cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer.TryAdd(inputsOfPlayer.Key,heighetsTick);
                }
            }

            return _cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer;
        }

        private int GetIndexOfEarliestInput(FixedUnorderedList<MatchMakingPlayerInputPacketC2S> inputs)
        {
            var span = inputs.AsSpan();
            int min = span[0].Tick;
            int indexOfMin = 0;
            for (int i = 1; i < span.Length; i++)
            {
                int v = span[i].Tick;

                if (v < min)
                {
                    min = v;
                    indexOfMin = i;
                }
            }

            return indexOfMin;
        }
        
        private int GetMaxHighestProcessedTickFromServer(FixedUnorderedList<MatchMakingPlayerInputPacketC2S> inputs)
        {
            var span = inputs.AsSpan();
            if (span.Length == 0)
                return 0;

            int max = span[0].HeighestProcessedTickFromServer;
            for (int i = 1; i < span.Length; i++)
            {
                int v = span[i].HeighestProcessedTickFromServer;
                if (v > max)
                    max = v;
            }

            return max;
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

        private void UpdatePlayerShoot(int processedTick, bool isShootInputPressed, MatchMakingPlayerStateS2C playerModel)
        {
            var shootState = playerModel.Spaceship.Shoot;
            var shouldShoot = isShootInputPressed && shootState.CooldownSecondsLeft == shootState.MaxCooldown;
            if (shouldShoot)
            {
                shootState.CooldownSecondsLeft -= _networkConfig.DeltaTime;
                playerModel.Spaceship.Shoot = shootState;
                CreateBulletForPlayer(processedTick, playerModel);
            }
        }

        private void CreateBulletForPlayer(int processedTick, MatchMakingPlayerStateS2C playerModel)
        {
            var bullet = _matchDataService.AddBullet(playerModel.Id, playerModel.Spaceship.Transform.GetHeadPosition(),
                playerModel.Spaceship.Transform.Direction, _gamePlayConfig.PlayerBullet.MoveSpeed, _gamePlayConfig.PlayerBullet.Radius);
            _netEventsDataService.AddBulletSpawnNetEvent(processedTick, bullet.Id, bullet.BelongToPlayerId, bullet.Position, bullet.Radius);
            _physicsSimulator.AddPlayerBullet(bullet.Id, playerModel.TeamId, bullet.Position, bullet.Velocity, bullet.Radius);
            LogService.LogTopic($"CreateBulletForPlayer {bullet.ToJson()}", LogTopicType.ServerNetwork);
        }

        private void UpdatePlayerDirection(MatchMakingPlayerInputPacketC2S playerInputPacket, MatchMakingPlayerStateS2C playerModel)
        {
            var rotationDelta = _gamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (playerInputPacket.IsMoveLeftInputPressed.ToInt() -
                 playerInputPacket.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerModel.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerModel.Spaceship.Transform.Direction = rotatedVector;
            if (playerInputPacket.IsMoveForwardInputPressed)
            {
                playerModel.Spaceship.Transform.Velocity = playerModel.Spaceship.Transform.Direction * _gamePlayConfig.PlayerSpaceship.TargetMovementSpeed;
            }
            else
            {
                playerModel.Spaceship.Transform.Velocity = System.Numerics.Vector2.Zero;
            }
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
        
        private CapacityDict<ushort, MatchMakingPlayerInputPacketC2S> PopEarliestInputsOfEachPlayer()
        {
            //_cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer.Clear();
            //_cachedProcessPlayersInputsResult.EarliestInputsPerPlayer.Clear();

            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.Players[i];
                var playerId = playerState.Id;
                MatchMakingPlayerInputPacketC2S earliestPlayerInput;
                if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs) && playerInputs.Count > 0)
                {
                    var indexOfEarliestInput = GetIndexOfEarliestInput(playerInputs);
                    earliestPlayerInput = playerInputs[indexOfEarliestInput];
                    playerInputs.RemoveAt(indexOfEarliestInput);
                    // if (playerInputs.Count == 0)
                    // {
                        //playerInputs.Clear();
                        //_inputsListsPool.Return(playerInputs);
                        //_inputsPerPlayer.Remove(playerId);
                    //}
                }
                else
                {
                    if (!TryGetCachedInputForPlayer(playerId, out earliestPlayerInput))
                    {
                        continue;
                    }
                }
                
                _cachedProcessPlayersInputsResult.EarliestInputsPerPlayer.Add(playerId, earliestPlayerInput);
            }

            return _cachedProcessPlayersInputsResult.EarliestInputsPerPlayer;
        }

        private bool TryGetCachedInputForPlayer(ushort playerId, out MatchMakingPlayerInputPacketC2S playerInputPacket)
        {
            return _lastProcessedInputPerPlayer.TryGetValue(playerId, out playerInputPacket);
        }
        
        public void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback)
        {
            var newPacket = _playerInputPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnPlayerInputReceived(newPacket, peer);
        }

        private void OnPlayerInputReceived(MatchMakingPlayerInputPacketC2S playerInputPacket, NetPeer peer)
        {
            var playerId = (ushort)peer.Tag;

            if (!_inputsPerPlayer.ContainsKey(playerId))
            {
                _inputsPerPlayer.Add(playerId, _inputsListsPool.Get());
            }
            ref var input = ref _inputsPerPlayer[playerId].AddAndGet();
            input = playerInputPacket;
            
            if (playerInputPacket.IsShootInputPressed)
            {
                //string time = DateTime.Now.ToString("HH:mm:ss.fff");

      //          Debug.Log($"{time} Shoot Received!! playerInputPacket:{playerInputPacket.ToJson()}, {_inputsPerPlayer[playerId].Count}, {_inputsPerPlayer.ToJson()}");
            }
            LogService.LogTopic($"Input packet received from player id {playerId}, input: {playerInputPacket.ToJson()}, inputs per player: {_inputsPerPlayer.ToJson()}", LogTopicType.ServerNetwork);
        }

        public void ManagedOnGUI()
        {
            GUILayout.Label("Inputs Per Player:");
            var playersCount = _inputsPerPlayer.Count;

            foreach (var kvp in _inputsPerPlayer)
            {
                GUILayout.Label($"Player ID: {kvp.Key}, Number of Inputs: {kvp.Value.Count}");
                if (playersCount != _inputsPerPlayer.Count) // added if the dict changes in another thread
                    return;
            }
        }

        public void ManagedOnDrawGizmos()
        {
            
        }
    }
    
    public class ProcessPlayersInputsResult
    {
        public CapacityDict<ushort, int> HeighestProcessedTickPerPlayer;
        public CapacityDict<ushort, MatchMakingPlayerInputPacketC2S> EarliestInputsPerPlayer;
        public ProcessPlayersInputsResult(int maxConcurrentPlayers)
        {
            HeighestProcessedTickPerPlayer = new CapacityDict<ushort, int>(maxConcurrentPlayers);
            EarliestInputsPerPlayer = new CapacityDict<ushort, MatchMakingPlayerInputPacketC2S>(maxConcurrentPlayers);
        }

        public void Clear()
        {
            HeighestProcessedTickPerPlayer.Clear();
            EarliestInputsPerPlayer.Clear();
        }
    }
}