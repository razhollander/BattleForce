using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public class MatchPlayerInputsPacketsHandler : IMatchPlayerInputsPacketsHandler, IGUIUpdatable
    {
        public PacketTypeC2S PacketType => PacketTypeC2S.MatchPlayersInput;

        private readonly IServerNetworkManager _networkManager;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;

        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly IRandomPlayersInputService _randomPlayersInputService;
        private readonly IClientsNetworkDataService _clientsNetworkDataService;

        private readonly CapacityDict<long, FixedClassUnorderedList<MatchPlayersInputPacketC2S>> _inputsPerClient;
        private readonly CapacityDict<long, int> _heighestProcessedTickPerClient;
        private readonly CapacityDict<long, MatchPlayersInputPacketC2S> _lastProcessedInputPerClient;
        private readonly CapacityDict<long, int> _lastProcessedInputTickPerClient;
        private readonly ConcurrentPool<MatchPlayersInputPacketC2S> _playerInputPacketsPool;
        private readonly ConcurrentPool<MatchPlayersInputPacketC2S> _earliestInputPacketsPool;
        private readonly ConcurrentPool<FixedClassUnorderedList<MatchPlayersInputPacketC2S>> _inputsListsPool;
        private readonly ProcessPlayersInputsResult _cachedProcessPlayersInputsResult;
        private readonly CapacityDict<long, MatchPlayersInputPacketC2S> _earliestInputsPerClient;
        private readonly IPlaybackRecorderService _playerbackRecorderService;
        private ApplyPlayerInputCommand _applyPlayerInputCommand;

        public bool DidReceiveAnyInputFromClient(long clientId)
        {
            return _inputsPerClient.ContainsKey(clientId);
        }
        
        public MatchPlayerInputsPacketsHandler(IServerNetworkManager networkManager,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IPlaybackRecorderService playerbackRecorderService, IClientsNetworkDataService clientsNetworkDataService,
            IRandomPlayersInputService randomPlayersInputService)
        {
            _networkManager = networkManager;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _playerbackRecorderService = playerbackRecorderService;
            _randomPlayersInputService = randomPlayersInputService;
            _clientsNetworkDataService = clientsNetworkDataService;
            _cachedProcessPlayersInputsResult = new ProcessPlayersInputsResult(networkConfig.MaxCap.ConcurrentPlayers);
            _earliestInputsPerClient = new CapacityDict<long, MatchPlayersInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _lastProcessedInputPerClient = new CapacityDict<long, MatchPlayersInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _lastProcessedInputTickPerClient = new CapacityDict<long, int>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerClient = new CapacityDict<long, FixedClassUnorderedList<MatchPlayersInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            var inputPacketsSavedPerPlayer = networkConfig.MaxCap.PlayersInputsPackets / networkConfig.MaxCap.ConcurrentPlayers;
            _inputsListsPool = new ConcurrentPool<FixedClassUnorderedList<MatchPlayersInputPacketC2S>>(() => new FixedClassUnorderedList<MatchPlayersInputPacketC2S>(inputPacketsSavedPerPlayer, () => new MatchPlayersInputPacketC2S(networkConfig.MaxCap.ConcurrentPlayers)), networkConfig.MaxCap.ConcurrentPlayers);
            var maxAmountOfInputPacketsPlusMaxClientsAmount = networkConfig.MaxCap.ConcurrentInputsProcessed + networkConfig.MaxCap.ConcurrentPlayers; // we use the pool in to two places, so for good order, combined their max caps
            _playerInputPacketsPool = new ConcurrentPool<MatchPlayersInputPacketC2S>(() => new MatchPlayersInputPacketC2S(networkConfig.MaxCap.ConcurrentPlayers), maxAmountOfInputPacketsPlusMaxClientsAmount);
            _earliestInputPacketsPool = new ConcurrentPool<MatchPlayersInputPacketC2S>(() => new MatchPlayersInputPacketC2S(networkConfig.MaxCap.ConcurrentPlayers), networkConfig.MaxCap.ConcurrentPlayers);
            _heighestProcessedTickPerClient = new CapacityDict<long, int>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
            _updateSubscriptionService.RegisterGuiUpdatable(this);
            _applyPlayerInputCommand = _commandFactory.CreateCommandVoid<ApplyPlayerInputCommand>();
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
            _updateSubscriptionService.UnregisterGuiUpdatable(this);
        }

        public ProcessPlayersInputsResult ProcessInputs(int processedTick, float deltaTime)
        {
            DiscardInputPacketsAlreadySuperseded();
            KeepLatestPacketsForBuffer(_networkConfig.ServerPlayerInputPacketsBuffer);
            _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient = GetHeighestProcessedTickFromServerPerClient();
            ProcessEarliestInputsPacketPerClient(processedTick, deltaTime);

            return _cachedProcessPlayersInputsResult;
        }

        // Applying a packet the server already moved past walks every held button back down and then up again on the next
        // packet, which reads as a fresh press. The jitter buffer still runs deliberately behind.
        private void DiscardInputPacketsAlreadySuperseded()
        {
            foreach (var kvp in _inputsPerClient)
            {
                if (!_lastProcessedInputTickPerClient.TryGetValue(kvp.Key, out var lastProcessedInputTick))
                {
                    continue;
                }

                DiscardInputPacketsNotNewerThanTick(kvp.Value, lastProcessedInputTick);
            }
        }

        private void DiscardInputPacketsNotNewerThanTick(FixedClassUnorderedList<MatchPlayersInputPacketC2S> inputsOfClient, int lastProcessedInputTick)
        {
            for (var i = inputsOfClient.Count - 1; i >= 0; i--)
            {
                var isPacketAlreadySuperseded = inputsOfClient[i].Tick <= lastProcessedInputTick;
                if (isPacketAlreadySuperseded)
                {
                    inputsOfClient.RemoveAt(i);
                }
            }
        }

        private void KeepLatestPacketsForBuffer(int bufferAmount)
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

        private void RemoveAmountOfEarliestInputs(FixedClassUnorderedList<MatchPlayersInputPacketC2S> inputsOfPlayer, int amountOfPacketsToRemove)
        {
            inputsOfPlayer.Sort();
            inputsOfPlayer.RemoveRange(0, amountOfPacketsToRemove);
        }

        private void ProcessEarliestInputsPacketPerClient(int processedTick, float deltaTime)
        {
            var earliestInputsPerClient = PopEarliestInputsOfEachClient();

            foreach (var kvp in earliestInputsPerClient)
            {
                var clientId = kvp.Key;
                var currentPacket = kvp.Value; 
                var clientNetworkData = _clientsNetworkDataService.ClientsNetworkDataDictionary[clientId];
                if (!_lastProcessedInputPerClient.TryGetValue(clientId, out var lastProcessedClientInsputs))
                {
                    _lastProcessedInputPerClient[clientId] = lastProcessedClientInsputs = _playerInputPacketsPool.Get();
                }
                
                lastProcessedClientInsputs.CopyFrom(currentPacket);
                _lastProcessedInputTickPerClient[clientId] = currentPacket.Tick;
                
                var playerInputs = currentPacket.PlayerInputs.AsSpan();
                for (var inputIndex = 0; inputIndex < playerInputs.Length; inputIndex++)
                {
                    var playerInput = playerInputs[inputIndex];
                    var playerId = playerInput.PlayerId;

                    if (!clientNetworkData.PlayerIds.Contains(playerId))
                    {
                        LogService.LogError("Player try to cheat and send inputs of different player! ClientId: " + clientId + " PlayerId: " + playerId + "");
                        continue;
                    }
                    
                    if (_gamePlayConfigService.GamePlayConfig.TestWithRandomPlayersInput)
                    {
                        _randomPlayersInputService.ApplyRandomInput(ref playerInput);
                    }

                    _applyPlayerInputCommand
                        .SetPlayerInputData(playerInput)
                        .SetClientId(clientId)
                        .SetProcessedTick(processedTick)
                        .SetDeltaTime(deltaTime)
                        .Execute();
                }
            }
        }

        private CapacityDict<long, int> GetHeighestProcessedTickFromServerPerClient()
        {
            _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient.Clear();

            foreach (var kvp in _heighestProcessedTickPerClient)
            {
                _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient.TryAdd(kvp.Key, kvp.Value);
            }

            return _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient;
        }

        private int GetIndexOfEarliestInput(FixedClassUnorderedList<MatchPlayersInputPacketC2S> inputs)
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

        
        private CapacityDict<long, MatchPlayersInputPacketC2S> PopEarliestInputsOfEachClient()
        {
            foreach (var kvp in _earliestInputsPerClient)
            {
                _earliestInputPacketsPool.Return(kvp.Value);
            }
            
            _earliestInputsPerClient.Clear();
            
            foreach (var clientId in _clientsNetworkDataService.ClientsNetworkDataDictionary.Keys)
            {
                MatchPlayersInputPacketC2S earliestClientInput;

                if (_inputsPerClient.TryGetValue(clientId, out var playersInputs) && playersInputs.Count > 0)
                {
                    earliestClientInput = _earliestInputPacketsPool.Get();
                    var indexOfEarliestInput = GetIndexOfEarliestInput(playersInputs);
                    earliestClientInput.CopyFrom(playersInputs[indexOfEarliestInput]);
                    playersInputs.RemoveAt(indexOfEarliestInput);
                }
                else
                {
                    if (!TryGetLastProcessedInputForClient(clientId, out var lastProcessedClientInput))
                    {
                        continue;
                    }
                    
                    earliestClientInput = _earliestInputPacketsPool.Get();
                    earliestClientInput.CopyFrom(lastProcessedClientInput);
                }
                
                _earliestInputsPerClient.Add(clientId, earliestClientInput);
            }

            return _earliestInputsPerClient;
        }

        private bool TryGetLastProcessedInputForClient(long clientId, out MatchPlayersInputPacketC2S playersInputPacket)
        {
            return _lastProcessedInputPerClient.TryGetValue(clientId, out playersInputPacket);
        }
        
        public void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback)
        {
            var newPacket = _playerInputPacketsPool.Get();
            newPacket.Deserialize(reader);
    
            var shouldIgnorePacket = !isReceivedFromPlayback && _playerbackRecorderService.IsPlaybackEnabled;
            var clientId = (long)peer.Tag;
            
            if (!shouldIgnorePacket)
            {
                OnClientInputsReceived(newPacket, clientId);
            }
            
            if(!isReceivedFromPlayback)
            {
                var heighestProcessedTickOfClient = _heighestProcessedTickPerClient.TryGetValue(clientId, out int value) ? value : -1;
                if (newPacket.HeighestProcessedTickFromServer > heighestProcessedTickOfClient)
                {
                    _heighestProcessedTickPerClient[clientId] = newPacket.HeighestProcessedTickFromServer;
                }
            }
            
            _playerInputPacketsPool.Return(newPacket);
        }

        private void OnClientInputsReceived(MatchPlayersInputPacketC2S playersInputPacket, long clientId)
        {
            if (!_inputsPerClient.ContainsKey(clientId))
            {
                _inputsPerClient.Add(clientId, _inputsListsPool.Get());
            }
    
            var inputsList = _inputsPerClient[clientId];
            var input = inputsList.AddAndGet();
            input.CopyFrom(playersInputPacket);
            LogService.LogTopic($"Input packet received from player id {clientId}, input: {playersInputPacket.ToJson()}, inputs per player: {_inputsPerClient.ToJson()}", LogTopicType.ServerNetwork);
        }

        public void ManagedOnGUI()
        {
            GUILayout.Label("Inputs Per Player:");
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

        public ProcessPlayersInputsResult(int maxConcurrentPlayers)
        {
            HeighestProcessedTickPerClient = new CapacityDict<long, int>(maxConcurrentPlayers);
        }
    }
}