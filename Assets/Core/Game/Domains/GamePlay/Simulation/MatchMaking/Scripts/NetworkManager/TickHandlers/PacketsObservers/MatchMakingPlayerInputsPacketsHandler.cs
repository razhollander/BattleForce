using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
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
        public PacketTypeC2S PacketType => PacketTypeC2S.MatchMakingPlayersInput;

        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchMakingDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IClientsNetworkDataService _clientsNetworkDataService;

        private readonly CapacityDict<long, FixedClassUnorderedList<MatchMakingPlayersInputPacketC2S>> _inputsPerClient;
        private readonly CapacityDict<long, int> _heighestProcessedTickPerClient;
        private readonly CapacityDict<long, MatchMakingPlayersInputPacketC2S> _lastProcessedInputPerClient;
        private readonly ConcurrentPool<MatchMakingPlayersInputPacketC2S> _playerInputPacketsPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<MatchMakingPlayersInputPacketC2S>> _inputsListsPool;
        private readonly ProcessPlayersInputsResult _cachedProcessPlayersInputsResult;

        public bool DidReceiveAnyInputFromClient(long clientId)
        {
            return _inputsPerClient.ContainsKey(clientId);
        }

        public MatchMakingPlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchMakingDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, INetEventsDataService netEventsDataService, IPhysicsSimulator physicsSimulator, IUpdateSubscriptionService updateSubscriptionService, IClientsNetworkDataService clientsNetworkDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _netEventsDataService = netEventsDataService;
            _physicsSimulator = physicsSimulator;
            _updateSubscriptionService = updateSubscriptionService;
            _clientsNetworkDataService = clientsNetworkDataService;
            _cachedProcessPlayersInputsResult = new ProcessPlayersInputsResult(networkConfig.MaxCap.ConcurrentPlayers);
            _lastProcessedInputPerClient = new CapacityDict<long, MatchMakingPlayersInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerClient = new CapacityDict<long, FixedClassUnorderedList<MatchMakingPlayersInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            var inputPacketsSavedPerPlayer = networkConfig.MaxCap.PlayersInputsPackets / networkConfig.MaxCap.ConcurrentPlayers;
            _inputsListsPool = new ConcurrentPool<FixedClassUnorderedList<MatchMakingPlayersInputPacketC2S>>(() => new FixedClassUnorderedList<MatchMakingPlayersInputPacketC2S>(inputPacketsSavedPerPlayer, () => new MatchMakingPlayersInputPacketC2S(networkConfig.MaxCap.ConcurrentPlayers)), networkConfig.MaxCap.ConcurrentPlayers);
            _playerInputPacketsPool = new ConcurrentPool<MatchMakingPlayersInputPacketC2S>(() => new MatchMakingPlayersInputPacketC2S(networkConfig.MaxCap.ConcurrentPlayers), networkConfig.MaxCap.ConcurrentInputsProcessed);
            _heighestProcessedTickPerClient = new CapacityDict<long, int>(networkConfig.MaxCap.ConcurrentPlayers);
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
            _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient = GetHeighestProcessedTickFromServerPerClient();
            _cachedProcessPlayersInputsResult.EarliestInputsPerClient = ProcessEarliestInputsPacketPerClient(processedTick);
            return _cachedProcessPlayersInputsResult;
        }
        
        private void LeaveLatestPacketsForBuffer(int bufferAmount)
        {
            foreach (var kvp in _inputsPerClient)
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

        private void RemoveAmountOfEarliestInputs(FixedClassUnorderedList<MatchMakingPlayersInputPacketC2S> inputsOfPlayer, int amountOfPacketsToRemove)
        {
            inputsOfPlayer.Sort();
            for (int i = amountOfPacketsToRemove - 1; i >= 0; i--)
            {
                inputsOfPlayer.RemoveAt(i);
            }
        }

        private CapacityDict<long, MatchMakingPlayersInputPacketC2S> ProcessEarliestInputsPacketPerClient(int processedTick)
        {
            var earliestInputsPerClient = PopEarliestInputsOfEachClient();

            foreach (var kvp in earliestInputsPerClient)
            {
                var clientId = kvp.Key;
                var currentPacket = kvp.Value;
                var clientNetworkData = _clientsNetworkDataService.ClientsNetworkDataDictionary[clientId];

                if (!_lastProcessedInputPerClient.TryGetValue(clientId, out var cachedPacket))
                {
                    cachedPacket = _playerInputPacketsPool.Get();
                    _lastProcessedInputPerClient[clientId] = cachedPacket;
                }
                cachedPacket.CopyFrom(currentPacket);

                foreach (var playerInput in currentPacket.PlayerInputs.AsSpan())
                {
                    var playerId = playerInput.PlayerId;

                    if (!clientNetworkData.PlayerIds.Contains(playerId))
                    {
                        LogService.LogError("Player try to cheat and send inputs of different player! ClientId: " + clientId + " PlayerId: " + playerId + "");
                        continue;
                    }

                    var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
                    if (playerState == null)
                    {
                        LogService.LogTopic($"Didn't find player state for player {playerId}!", LogTopicType.ServerNetwork);
                        continue;
                    }

                    UpdatePlayerDirection(playerInput, playerState);
                    UpdatePlayerShoot(processedTick, playerInput.IsShootInputPressed, playerState);
                }
            }

            return earliestInputsPerClient;
        }
        
        private CapacityDict<long, int> GetHeighestProcessedTickFromServerPerClient()
        {
            foreach (var kvp in _heighestProcessedTickPerClient)
            {
                _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient.TryAdd(kvp.Key, kvp.Value);
            }

            return _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient;
        }

        private int GetIndexOfEarliestInput(FixedClassUnorderedList<MatchMakingPlayersInputPacketC2S> inputs)
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
                playerModel.Spaceship.Transform.Direction, _gamePlayConfigService.GamePlayConfig.PlayerBullet.MoveSpeed, _gamePlayConfigService.GamePlayConfig.PlayerBullet.Radius);
            _netEventsDataService.AddBulletSpawnNetEvent(processedTick, bullet.Id, bullet.BelongToPlayerId, bullet.Position, bullet.Radius, bullet.Velocity);
            _physicsSimulator.AddPlayerBullet(bullet.Id, playerModel.TeamId, bullet.Position, bullet.Velocity, bullet.Radius);
            LogService.LogTopic($"CreateBulletForPlayer {bullet.ToJson()}", LogTopicType.ServerNetwork);
        }

        private void UpdatePlayerDirection(MatchMakingLocalPlayerInputDataC2S playerInputData, MatchMakingPlayerStateS2C playerModel)
        {
            var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (playerInputData.IsMoveLeftInputPressed.ToInt() -
                 playerInputData.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerModel.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerModel.Spaceship.Transform.Direction = rotatedVector;
            
            if (playerInputData.IsMoveForwardInputPressed)
            {
                playerModel.Spaceship.Transform.Velocity = playerModel.Spaceship.Transform.Direction * _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.TargetMovementSpeed;
            }
            else
            {
                playerModel.Spaceship.Transform.Velocity = System.Numerics.Vector2.Zero;
            }
        }
        
        private CapacityDict<long, MatchMakingPlayersInputPacketC2S> PopEarliestInputsOfEachClient()
        {
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                var clientId = kvp.Key;
                MatchMakingPlayersInputPacketC2S earliestPlayersInput;
                if (_inputsPerClient.TryGetValue(clientId, out var playersInputs) && playersInputs.Count > 0)
                {
                    var indexOfEarliestInput = GetIndexOfEarliestInput(playersInputs);
                    earliestPlayersInput = playersInputs[indexOfEarliestInput];
                    playersInputs.RemoveAt(indexOfEarliestInput);
                }
                else
                {
                    if (!TryGetCachedInputForClient(clientId, out earliestPlayersInput))
                    {
                        continue;
                    }
                }
                
                _cachedProcessPlayersInputsResult.EarliestInputsPerClient.Add(clientId, earliestPlayersInput);
            }

            return _cachedProcessPlayersInputsResult.EarliestInputsPerClient;
        }

        private bool TryGetCachedInputForClient(long clientId, out MatchMakingPlayersInputPacketC2S playersInputPacket)
        {
            return _lastProcessedInputPerClient.TryGetValue(clientId, out playersInputPacket);
        }
        
        public void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback)
        {
            var newPacket = _playerInputPacketsPool.Get();
            newPacket.Deserialize(reader);
            
            var clientId = (long)peer.Tag;
            var heighestProcessedTickOfClient = _heighestProcessedTickPerClient.TryGetValue(clientId, out int value) ? value : -1;
            
            if (newPacket.HeighestProcessedTickFromServer > heighestProcessedTickOfClient)
            {
                _heighestProcessedTickPerClient[clientId] = newPacket.HeighestProcessedTickFromServer;
            }
            
            OnClientInputsReceived(newPacket, clientId);

            _playerInputPacketsPool.Return(newPacket);
        }
        private void OnClientInputsReceived(MatchMakingPlayersInputPacketC2S playersInputPacket, long clientId)
        {
            if (!_inputsPerClient.ContainsKey(clientId))
            {
                _inputsPerClient.Add(clientId, _inputsListsPool.Get());
            }
            
            var inputsList = _inputsPerClient[clientId];
            var input = inputsList.AddAndGet();
            input.CopyFrom(playersInputPacket);
            
            LogService.LogTopic($"Input packet received from client id {clientId}, input: {playersInputPacket.ToJson()}, inputs per client: {_inputsPerClient.ToJson()}", LogTopicType.ServerNetwork);
        }

        public void ManagedOnGUI()
        {
            GUILayout.Label("Inputs Per Client:");
            var clientsCount = _inputsPerClient.Count;

            foreach (var kvp in _inputsPerClient)
            {
                GUILayout.Label($"Client ID: {kvp.Key}, Number of Inputs: {kvp.Value.Count}");
                if (clientsCount != _inputsPerClient.Count) // added if the dict changes in another thread
                    return;
            }
        }

        public void ManagedOnDrawGizmos()
        {
            
        }
    }
    
    public class ProcessPlayersInputsResult
    {
        public CapacityDict<long, int> HeighestProcessedTickPerClient;
        public CapacityDict<long, MatchMakingPlayersInputPacketC2S> EarliestInputsPerClient;
        
        public ProcessPlayersInputsResult(int maxConcurrentPlayers)
        {
            HeighestProcessedTickPerClient = new CapacityDict<long, int>(maxConcurrentPlayers);
            EarliestInputsPerClient = new CapacityDict<long, MatchMakingPlayersInputPacketC2S>(maxConcurrentPlayers);
        }

        public void Clear()
        {
            HeighestProcessedTickPerClient.Clear();
            EarliestInputsPerClient.Clear();
        }
    }
}