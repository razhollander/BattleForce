using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
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

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly ISimulationInputService _simulationInputService;
        private readonly IClientsNetworkDataService _clientsNetworkDataService;

        private readonly CapacityDict<long, FixedUnorderedList<MatchPlayersInputPacketC2S>> _inputsPerClient;
        private readonly CapacityDict<long, int> _heighestProcessedTickPerClient;
        private readonly CapacityDict<long, MatchPlayersInputPacketC2S> _lastProcessedInputPerClient;
        private readonly ConcurrentPool<MatchPlayersInputPacketC2S> _playerInputPacketsPool;
        private readonly ConcurrentPool<FixedUnorderedList<MatchPlayersInputPacketC2S>> _inputsListsPool;
        private readonly ProcessPlayersInputsResult _cachedProcessPlayersInputsResult;
        private readonly IPlayersTalentsManager _playersTalentsManager;
        private readonly IPlaybackRecorderService _playerbackRecorderService;
        private readonly TryPerformShootForPlayerIfNotOnCooldownCommand _tryPerformShootForPlayerIfNotOnCooldownCommand;

        public bool DidReceiveAnyInputFromClient(long clientId)
        {
            return _inputsPerClient.ContainsKey(clientId);
        }
        
        public MatchPlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, INetEventsDataService netEventsDataService, IPhysicsSimulator physicsSimulator, IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IPlayersTalentsManager playersTalentsManager, IPlaybackRecorderService playerbackRecorderService, ISimulationInputService simulationInputService, IClientsNetworkDataService clientsNetworkDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _netEventsDataService = netEventsDataService;
            _physicsSimulator = physicsSimulator;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _playersTalentsManager = playersTalentsManager;
            _playerbackRecorderService = playerbackRecorderService;
            _simulationInputService = simulationInputService;
            _clientsNetworkDataService = clientsNetworkDataService;
            _cachedProcessPlayersInputsResult = new ProcessPlayersInputsResult(networkConfig.MaxCap.ConcurrentPlayers);
            _lastProcessedInputPerClient = new CapacityDict<long, MatchPlayersInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerClient = new CapacityDict<long, FixedUnorderedList<MatchPlayersInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            var inputPacketsSavedPerPlayer = networkConfig.MaxCap.PlayersInputsPackets / networkConfig.MaxCap.ConcurrentPlayers;
            _inputsListsPool = new ConcurrentPool<FixedUnorderedList<MatchPlayersInputPacketC2S>>(() => new FixedUnorderedList<MatchPlayersInputPacketC2S>(inputPacketsSavedPerPlayer), networkConfig.MaxCap.ConcurrentPlayers);
            _playerInputPacketsPool = new ConcurrentPool<MatchPlayersInputPacketC2S>(() => new MatchPlayersInputPacketC2S(), networkConfig.MaxCap.ConcurrentInputsProcessed);
            _heighestProcessedTickPerClient = new CapacityDict<long, int>(networkConfig.MaxCap.ConcurrentPlayers);
            _tryPerformShootForPlayerIfNotOnCooldownCommand = _commandFactory.CreateCommandVoid<TryPerformShootForPlayerIfNotOnCooldownCommand>();
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

        public ProcessPlayersInputsResult ProcessInputs(int processedTick, float deltaTime)
        {
            _cachedProcessPlayersInputsResult.Clear();
            LeaveLatestPacketsForBuffer(_networkConfig.ServerPlayerInputPacketsBuffer);
            _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient = GetHeighestProcessedTickFromServerPerClient();
            _cachedProcessPlayersInputsResult.EarliestInputsPerClient = ProcessEarliestInputsPacketPerClient(processedTick, deltaTime); // todo move to a new command?

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

        private void RemoveAmountOfEarliestInputs(FixedUnorderedList<MatchPlayersInputPacketC2S> inputsOfPlayer, int amountOfPacketsToRemove)
        {
            inputsOfPlayer.Sort();
            for (int i = amountOfPacketsToRemove - 1; i >= 0; i--)
            {
                _playerInputPacketsPool.Return(inputsOfPlayer[i]);
                inputsOfPlayer.RemoveAt(i);
            }
        }

        private CapacityDict<long, MatchPlayersInputPacketC2S> ProcessEarliestInputsPacketPerClient(int processedTick, float deltaTime)
        {
            var earliestInputsPerClient = PopEarliestInputsOfEachClient();

            foreach (var kvp in earliestInputsPerClient)
            {
                var clientId = kvp.Key;
                var clientNetworkData = _clientsNetworkDataService.ClientsNetworkDataDictionary[clientId];

                foreach (var playerInput in kvp.Value.PlayerInputs.AsSpan())
                {
                    var playerId = playerInput.PlayerId;

                    if (!clientNetworkData.PlayerIds.Contains(playerId))
                    {
                        LogService.LogError("Player try to cheat and send inputs of different player! ClientId: " + clientId + " PlayerId: " + playerId + "");
                        continue;
                    }

                    var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
                    UpdatePlayerShoot(processedTick, true, playerState);

                    playerState.Spaceship.TalentsState.AimDirection = playerInput.AimDirection;
                    UpdatePlayerDirection(playerInput, playerState);
                    bool isTalentAInputPressed = playerInput.IsTalentAInputPressed;
                    bool isTalentBInputPressed = playerInput.IsTalentBInputPressed;
                    bool isTalentCInputPressed = playerInput.IsTalentCInputPressed;

                    if (_lastProcessedInputPerClient.TryGetValue(playerId, out var lastPlayerInput))
                    {
                        _playerInputPacketsPool.Return(lastPlayerInput);
                    }

                    _lastProcessedInputPerClient[playerId] = kvp.Value;


                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentAInput, isTalentAInputPressed);
                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentBInput, isTalentBInputPressed);
                    _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentCInput, isTalentCInputPressed);

                    TrySwitchTalent(processedTick, playerState);
                    ProcessPlayerTalentInput(processedTick, isTalentAInputPressed, isTalentBInputPressed, isTalentCInputPressed, playerState, deltaTime);
                }

            }
            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
              
            }

            return earliestInputsPerClient;
        }

        private void ProcessPlayerTalentInput(int processedTick, bool isTalentAInputPressed, bool isTalentBInputPressed, bool isTalentCInputPressed, PlayerStateS2C playerState, float deltaTime)
        {
            if (!playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalent))
            {
                return;
            }

            var playerId = playerState.Id;
            _playersTalentsManager.ProcessAllTalentsTickOfPlayer(playerId, processedTick, deltaTime); // not sure should be here
            
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
                if (_playersTalentsManager.TrySwitchToTalent(playerId, talentAIndex))
                {
                    didSwitchToAnyTalent = true;
                    switchedTalentIndex = talentAIndex;
                }
            }
            if (wasTalentBInputDownThisTick && currentSelectedTalentIndex != talentBIndex)
            {
                if (_playersTalentsManager.TrySwitchToTalent(playerId, talentBIndex))
                {
                    didSwitchToAnyTalent = true;
                    switchedTalentIndex = talentBIndex;
                }
            }
            if (wasTalentCInputDownThisTick && currentSelectedTalentIndex != talentCIndex)
            {
                if (_playersTalentsManager.TrySwitchToTalent(playerId, talentCIndex))
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
            _cachedProcessPlayersInputsResult.Clear();

            foreach (var kvp in _heighestProcessedTickPerClient)
            {
                _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient.TryAdd(kvp.Key, kvp.Value);
            }

            return _cachedProcessPlayersInputsResult.HeighestProcessedTickPerClient;
        }

        private int GetIndexOfEarliestInput(FixedUnorderedList<MatchPlayersInputPacketC2S> inputs)
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
            var shootState = playerModel.Spaceship.Shoot;
            var isReadyToShoot = false;//shootState.CooldownSecondsLeft == shootState.MaxCooldown;

            if (!isReadyToShoot)
            {
                return;
            }

            var isEnemyInfrontOfPlayer = _physicsSimulator.ArcCastOnPlayers(
                playerModel.Spaceship.Transform.Position,
                _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.AutoShootRange,
                playerModel.Spaceship.Transform.Direction,
                _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.AutoShootAngleDegrees,
                (short)playerModel.TeamId,
                out var hitBodyData);

            if (!isEnemyInfrontOfPlayer)
            {
                return;
            }

            var enemyId = hitBodyData.Id;
            var enemyPlayerModel = _matchDataService.SimulationState.GetPlayerById(enemyId);
            var doesPlayersLookAtSameDirection = System.Numerics.Vector2.Dot(playerModel.Spaceship.Transform.Direction, enemyPlayerModel.Spaceship.Transform.Direction) > 0;
            if (doesPlayersLookAtSameDirection)
            {
                _tryPerformShootForPlayerIfNotOnCooldownCommand.SetPlayerId(playerId).SetTick(processedTick).Execute();
            }
        }

        private void UpdatePlayerDirection(MatchLocalPlayerInputDataC2S playerInputData, PlayerStateS2C playerState)
        {
            if (playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) && selectedTalent.IsCurrentlyAiming)
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
        
        private CapacityDict<long, MatchPlayersInputPacketC2S> PopEarliestInputsOfEachClient()
        {
            foreach (var kvp in _clientsNetworkDataService.ClientsNetworkDataDictionary)
            {
                var clientId = kvp.Key;
                MatchPlayersInputPacketC2S earliestPlayersInput;
                if (_inputsPerClient.TryGetValue(clientId, out var playersInputs) && playersInputs.Count > 0)
                {
                    var indexOfEarliestInput = GetIndexOfEarliestInput(playersInputs);
                    earliestPlayersInput = playersInputs[indexOfEarliestInput];
                    playersInputs.RemoveAt(indexOfEarliestInput);
                    // if (playerInputs.Count == 0)
                    // {
                        //playerInputs.Clear();
                        //_inputsListsPool.Return(playerInputs);
                        //_inputsPerPlayer.Remove(playerId);
                    //}
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

        private bool TryGetCachedInputForClient(long clientId, out MatchPlayersInputPacketC2S playersInputPacket)
        {
            return _lastProcessedInputPerClient.TryGetValue(clientId, out playersInputPacket);
        }
        
        public void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback)
        {
            var newPacket = _playerInputPacketsPool.Get();
            newPacket.Deserialize(reader);
            
            var shouldIgnorePacket = !isReceivedFromPlayback && _playerbackRecorderService.IsPlaybackEnabled;
            if (shouldIgnorePacket)
            {
                _playerInputPacketsPool.Return(newPacket);
                return;
            }
            
            var clientId = (long)peer.Tag;
            var heighestProcessedTickOfClient = _heighestProcessedTickPerClient.TryGetValue(clientId, out int value) ? value : -1;
            if (newPacket.HeighestProcessedTickFromServer > heighestProcessedTickOfClient)
            {
                _heighestProcessedTickPerClient[clientId] = newPacket.HeighestProcessedTickFromServer;
            }
            OnClientInputsReceived(newPacket, clientId);
        }

        private void OnClientInputsReceived(MatchPlayersInputPacketC2S playersInputPacket, long clientId)
        {
            if (!_inputsPerClient.ContainsKey(clientId))
            {
                _inputsPerClient.Add(clientId, _inputsListsPool.Get());
            }
    
            var inputsList = _inputsPerClient[clientId];
            ref var input = ref inputsList.AddAndGet();
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
        public CapacityDict<long, MatchPlayersInputPacketC2S> EarliestInputsPerClient;
        public ProcessPlayersInputsResult(int maxConcurrentPlayers)
        {
            HeighestProcessedTickPerClient = new CapacityDict<long, int>(maxConcurrentPlayers);
            EarliestInputsPerClient = new CapacityDict<long, MatchPlayersInputPacketC2S>(maxConcurrentPlayers);
        }

        public void Clear()
        {
            HeighestProcessedTickPerClient.Clear();
            EarliestInputsPerClient.Clear();
        }
    }
}