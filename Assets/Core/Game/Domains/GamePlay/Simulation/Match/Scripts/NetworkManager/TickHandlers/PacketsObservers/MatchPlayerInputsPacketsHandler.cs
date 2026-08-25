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
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly ISimulationInputService _simulationInputService;
        private readonly IRandomPlayersInputService _randomPlayersInputService;
        private readonly IPlayersMouseDataService _playersMouseDataService;
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
        private readonly IPlayersTalentsManager _playersTalentsManager;
        private readonly IPlayersPowerUpsManager _playersPowerUpsManager;
        private readonly IPlaybackRecorderService _playerbackRecorderService;
        private TryShootLockedOnTargetsCommand _tryShootLockedOnTargetsCommand;
        private TryPerformBarrelDashCommand _tryPerformBarrelDashCommand;
        private TrySetPlayerMoveDestinationPointCommand _trySetPlayerMoveDestinationPointCommand;
        private RotatePlayerTowardsMoveDestinationPointCommand _rotatePlayerTowardsMoveDestinationPointCommand;

        public bool DidReceiveAnyInputFromClient(long clientId)
        {
            return _inputsPerClient.ContainsKey(clientId);
        }
        
        public MatchPlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, INetEventsDataService netEventsDataService, IPhysicsSimulator physicsSimulator, IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IPlayersTalentsManager playersTalentsManager, IPlaybackRecorderService playerbackRecorderService, ISimulationInputService simulationInputService, IClientsNetworkDataService clientsNetworkDataService,
            IPlayersPowerUpsManager playersPowerUpsManager, IPlayersMouseDataService playersMouseDataService, IRandomPlayersInputService randomPlayersInputService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _netEventsDataService = netEventsDataService;
            _physicsSimulator = physicsSimulator;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _playersTalentsManager = playersTalentsManager;
            _playersPowerUpsManager = playersPowerUpsManager;
            _playerbackRecorderService = playerbackRecorderService;
            _simulationInputService = simulationInputService;
            _randomPlayersInputService = randomPlayersInputService;
            _playersMouseDataService = playersMouseDataService;
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
            _tryShootLockedOnTargetsCommand = _commandFactory.CreateCommandVoid<TryShootLockedOnTargetsCommand>();
            _tryPerformBarrelDashCommand = _commandFactory.CreateCommandVoid<TryPerformBarrelDashCommand>();
            _trySetPlayerMoveDestinationPointCommand = _commandFactory.CreateCommandVoid<TrySetPlayerMoveDestinationPointCommand>();
            _rotatePlayerTowardsMoveDestinationPointCommand = _commandFactory.CreateCommandVoid<RotatePlayerTowardsMoveDestinationPointCommand>();
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
            ProcessEarliestInputsPacketPerClient(processedTick, deltaTime); // todo move to a new command?

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

                    var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);

                    bool isTalentAInputPressed = playerInput.IsTalentAInputPressed;
                    bool isTalentBInputPressed = playerInput.IsTalentBInputPressed;
                    bool isTalentCInputPressed = playerInput.IsTalentCInputPressed;

                    UpdatePlayerShoot(processedTick, playerInput.IsShootInputPressed, playerState);

                    playerState.Spaceship.AimDirection = playerInput.AimDirection;
                    _playersMouseDataService.SetPlayerMouseData(playerId, playerInput.IsUsingMouseAim, playerInput.MouseWorldPosition);
                    UpdatePlayerDirection(processedTick, clientId, playerInput, playerState);

                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentAInput, isTalentAInputPressed);
                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentBInput, isTalentBInputPressed);
                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentCInput, isTalentCInputPressed);
                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.PowerUpInput, playerInput.IsPowerUpInputPressed);
                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.BarrelDashInput, playerInput.IsBarrelDashInputPressed);

                    TrySwitchTalent(processedTick, playerState);
                    ProcessPlayerTalentInput(processedTick, isTalentAInputPressed, isTalentBInputPressed, isTalentCInputPressed, playerState, deltaTime);
                    ProcessPlayerPowerUpInput(processedTick, playerState);
                    ProcessPlayerBarrelDashInput(processedTick, playerId);
                }
            }
        }

        private void ProcessPlayerPowerUpInput(int processedTick, PlayerStateS2C playerState)
        {
            var playerId = playerState.Id;
            var wasPowerUpInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.PowerUpInput);
            _playersPowerUpsManager.ProcessPowerUpInput(playerId, processedTick, wasPowerUpInputDownThisTick);
        }

        private void ProcessPlayerBarrelDashInput(int processedTick, ushort playerId)
        {
            var wasBarrelDashInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.BarrelDashInput);
            if (!wasBarrelDashInputDownThisTick)
            {
                return;
            }

            _tryPerformBarrelDashCommand.SetPlayerId(playerId).SetProcessedTick(processedTick).Execute();
        }

        private void ProcessPlayerTalentInput(int processedTick, bool isTalentAInputPressed, bool isTalentBInputPressed, bool isTalentCInputPressed, PlayerStateS2C playerState, float deltaTime)
        {
            if (!playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalent))
            {
                return;
            }

            var playerId = playerState.Id;

            if (_playersPowerUpsManager.IsPowerUpActiveForPlayer(playerId))
                return;

            if (currentSelectedTalent.IsOnCooldown())
            {
                return;
            }
            
            var currentSelectedTalentIndex = playerState.Spaceship.TalentsState.SelectedTalentIndex;
            var selectedTalentInputType = PlayerInputType.TalentAInput;
            var isSelectedTalentInputPressed = false;
            switch (currentSelectedTalentIndex)
            {
                case 0: 
                    selectedTalentInputType = PlayerInputType.TalentAInput;
                    isSelectedTalentInputPressed = isTalentAInputPressed;
                    break;
                case 1: 
                    selectedTalentInputType = PlayerInputType.TalentBInput;
                    isSelectedTalentInputPressed = isTalentBInputPressed;
                    break;
                case 2: 
                    selectedTalentInputType = PlayerInputType.TalentCInput;
                    isSelectedTalentInputPressed = isTalentCInputPressed;
                    break;
            }
            
            var wasSelectedTalentInputReleased = _simulationInputService.WasInputReleasedThisTick(playerId, selectedTalentInputType);
            var wasSelectedTalentInputDown = _simulationInputService.WasInputDownThisTick(playerId, selectedTalentInputType);
            
            _playersTalentsManager.ProcessPlayerTalentInput(playerId, currentSelectedTalent.TalentType, processedTick, wasSelectedTalentInputDown, isSelectedTalentInputPressed, wasSelectedTalentInputReleased, deltaTime);
        }

        private void TrySwitchTalent(int processedTick, PlayerStateS2C playerState)
        {
            bool doesPlayerHaveLessThan2Talents = playerState.Spaceship.TalentsState.Talents.Count < 2;
            if (doesPlayerHaveLessThan2Talents)
            {
                return;
            }
            
            var playerId = playerState.Id;
            var wasTalentAInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.TalentAInput);
            var wasTalentBInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.TalentBInput);
            var wasTalentCInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.TalentCInput);
            var currentSelectedTalentIndex = playerState.Spaceship.TalentsState.SelectedTalentIndex;
            var talentAIndex = 0;
            var talentBIndex = 1;
            var talentCIndex = 2;
            var didSwitchToAnyTalent = false;
            var switchedTalentIndex = -1;
            
            if (wasTalentAInputDownThisTick && currentSelectedTalentIndex != talentAIndex)
            {
                if (_playersTalentsManager.TrySwitchToTalent(playerId, talentAIndex, processedTick))
                {
                    didSwitchToAnyTalent = true;
                    switchedTalentIndex = talentAIndex;
                }
            }
            if (wasTalentBInputDownThisTick && currentSelectedTalentIndex != talentBIndex)
            {
                if (_playersTalentsManager.TrySwitchToTalent(playerId, talentBIndex, processedTick))
                {
                    didSwitchToAnyTalent = true;
                    switchedTalentIndex = talentBIndex;
                }
            }
            if (wasTalentCInputDownThisTick && currentSelectedTalentIndex != talentCIndex)
            {
                if (_playersTalentsManager.TrySwitchToTalent(playerId, talentCIndex, processedTick))
                {
                    didSwitchToAnyTalent = true;
                    switchedTalentIndex = talentCIndex;
                }
            }

            if (didSwitchToAnyTalent)
            {
                _netEventsDataService.AddTalentSwitchNetEvent(processedTick, playerId, switchedTalentIndex);
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

        private void UpdatePlayerShoot(int processedTick, bool isShootInputPressed, PlayerStateS2C playerModel)
        {
            var playerId = playerModel.Id;
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.Shoot, isShootInputPressed);

            var wasShootInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.Shoot);
            if (!_gamePlayConfigService.GamePlayConfig.IsAutoShoot && !wasShootInputDownThisTick)
            {
                return;
            }

            _tryShootLockedOnTargetsCommand.SetCasterPlayerId(playerId).SetProcessedTick(processedTick).Execute();
        }

        private void UpdatePlayerDirection(int processedTick, long clientId, MatchLocalPlayerInputDataC2S playerInputData, PlayerStateS2C playerState)
        {
            if (IsPlayerSteeringWithMouse(playerInputData))
            {
                UpdatePlayerDirectionFromMoveDestinationPoint(processedTick, clientId, playerInputData, playerState);
                return;
            }

            if (playerState.Spaceship.TalentsState.IsSelectedTalentBlockingRotation())
            {
                return;
            }
            
            var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (playerInputData.IsMoveLeftInputPressed.ToInt() -
                 playerInputData.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerState.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerState.Spaceship.Transform.Direction = rotatedVector;
        }

        private bool IsPlayerSteeringWithMouse(MatchLocalPlayerInputDataC2S playerInputData)
        {
            var isPlayerOnKeyboard = playerInputData.IsUsingMouseAim;

            return _sharedGamePlayConfig.ShouldMoveWithMouse && isPlayerOnKeyboard;
        }

        private void UpdatePlayerDirectionFromMoveDestinationPoint(int processedTick, long clientId, MatchLocalPlayerInputDataC2S playerInputData, PlayerStateS2C playerState)
        {
            var playerId = playerState.Id;
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.MoveToPointInput, playerInputData.IsMoveToPointInputPressed);

            var isRetargetingDestinationPoint = _simulationInputService.IsInputPressed(playerId, PlayerInputType.MoveToPointInput);
            if (isRetargetingDestinationPoint)
            {
                var wasDestinationPointClickedThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.MoveToPointInput);
                _trySetPlayerMoveDestinationPointCommand
                    .SetPlayerId(playerId)
                    .SetDestinationPoint(playerInputData.MouseWorldPosition)
                    .SetClientId(clientId)
                    .SetProcessedTick(processedTick)
                    .ShouldShowIndicator(wasDestinationPointClickedThisTick)
                    .Execute();
            }

            _rotatePlayerTowardsMoveDestinationPointCommand.SetPlayerId(playerId).Execute();
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