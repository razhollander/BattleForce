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
        public PacketTypeC2S PacketType => PacketTypeC2S.MatchPlayerInput;

        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly ISimulationInputService _simulationInputService;

        private readonly CapacityDict<ushort, FixedUnorderedList<MatchPlayerInputPacketC2S>> _inputsPerPlayer;
        private readonly CapacityDict<ushort, int> _heighestProcessedTickPerPlayer;
        private readonly CapacityDict<ushort, MatchPlayerInputPacketC2S> _lastProcessedInputPerPlayer;
        private readonly ConcurrentPool<MatchPlayerInputPacketC2S> _playerInputPacketsPool;
        private readonly ConcurrentPool<FixedUnorderedList<MatchPlayerInputPacketC2S>> _inputsListsPool;
        private readonly ProcessPlayersInputsResult _cachedProcessPlayersInputsResult;
        private readonly IPlayersTalentsManager _playersTalentsManager;
        private readonly IPlaybackRecorderService _playerbackRecorderService;
        private readonly TryPerformShootForPlayerIfNotOnCooldownCommand _tryPerformShootForPlayerIfNotOnCooldownCommand;

        public bool DidReceiveAnyInputFromPlayer(ushort playerId)
        {
            return _inputsPerPlayer.ContainsKey(playerId);
        }
        
        public MatchPlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, INetEventsDataService netEventsDataService, IPhysicsSimulator physicsSimulator, IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IPlayersTalentsManager playersTalentsManager, IPlaybackRecorderService playerbackRecorderService, ISimulationInputService simulationInputService)
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
            _cachedProcessPlayersInputsResult = new ProcessPlayersInputsResult(networkConfig.MaxCap.ConcurrentPlayers);
            _lastProcessedInputPerPlayer = new CapacityDict<ushort, MatchPlayerInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<MatchPlayerInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            var inputPacketsSavedPerPlayer = networkConfig.MaxCap.PlayersInputsPackets / networkConfig.MaxCap.ConcurrentPlayers;
            _inputsListsPool = new ConcurrentPool<FixedUnorderedList<MatchPlayerInputPacketC2S>>(() => new FixedUnorderedList<MatchPlayerInputPacketC2S>(inputPacketsSavedPerPlayer), networkConfig.MaxCap.ConcurrentPlayers);
            _playerInputPacketsPool = new ConcurrentPool<MatchPlayerInputPacketC2S>(() => new MatchPlayerInputPacketC2S(), networkConfig.MaxCap.ConcurrentInputsProcessed);
            _heighestProcessedTickPerPlayer = new CapacityDict<ushort, int>(networkConfig.MaxCap.ConcurrentPlayers);
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
            _cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer = GetHeighestProcessedTickFromServerPerPlayer();
            _cachedProcessPlayersInputsResult.EarliestInputsPerPlayer = ProcessEarliestInputPerPlayers(processedTick, deltaTime); // todo move to a new command?

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

        private void RemoveAmountOfEarliestInputs(FixedUnorderedList<MatchPlayerInputPacketC2S> inputsOfPlayer, int amountOfPacketsToRemove)
        {
            inputsOfPlayer.Sort();
            for (int i = amountOfPacketsToRemove - 1; i >= 0; i--)
            {
                _playerInputPacketsPool.Return(inputsOfPlayer[i]);
                inputsOfPlayer.RemoveAt(i);
            }
        }

        private CapacityDict<ushort, MatchPlayerInputPacketC2S> ProcessEarliestInputPerPlayers(int processedTick, float deltaTime)
        {
            var earliestInputPerPlayers = PopEarliestInputsOfEachPlayer();

            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.GetPlayerByIndex(i);
                var playerId = playerState.Id;
                UpdatePlayerShoot(processedTick, true, playerState);
                var isTalentAInputPressed = false;
                var isTalentBInputPressed = false;
                var isTalentCInputPressed = false;
                if (earliestInputPerPlayers.TryGetValue(playerId, out var playerInputPacket))
                {
                    playerState.Spaceship.TalentsState.AimDirection = playerInputPacket.AimDirection;
                    UpdatePlayerDirection(playerInputPacket, playerState);
                    isTalentAInputPressed = playerInputPacket.IsTalentAInputPressed;
                    isTalentBInputPressed = playerInputPacket.IsTalentBInputPressed;
                    isTalentCInputPressed = playerInputPacket.IsTalentCInputPressed;
                    
                    if (_lastProcessedInputPerPlayer.TryGetValue(playerId, out var lastPlayerInput))
                    {
                        _playerInputPacketsPool.Return(lastPlayerInput);
                    }
                    _lastProcessedInputPerPlayer[playerId] = playerInputPacket;
                }
                
                _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentAInput, isTalentAInputPressed);
                _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentBInput, isTalentBInputPressed);
                _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentCInput, isTalentCInputPressed);
                
                TrySwitchTalent(processedTick, playerState);
                ProcessPlayerTalentInput(processedTick, isTalentAInputPressed, isTalentBInputPressed, isTalentCInputPressed, playerState, deltaTime);
            }

            return earliestInputPerPlayers;
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

        private CapacityDict<ushort, int> GetHeighestProcessedTickFromServerPerPlayer()
        {
            _cachedProcessPlayersInputsResult.Clear();

            foreach (var kvp in _heighestProcessedTickPerPlayer)
            {
                _cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer.TryAdd(kvp.Key, kvp.Value);
            }

            return _cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer;
        }

        private int GetIndexOfEarliestInput(FixedUnorderedList<MatchPlayerInputPacketC2S> inputs)
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

        private void UpdatePlayerDirection(MatchPlayerInputPacketC2S playerInputPacket, PlayerStateS2C playerState)
        {
            if (playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) && selectedTalent.IsCurrentlyAiming)
            {
                return;
            }
            
            var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (playerInputPacket.IsMoveLeftInputPressed.ToInt() -
                 playerInputPacket.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerState.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerState.Spaceship.Transform.Direction = rotatedVector;
        }
        
        private CapacityDict<ushort, MatchPlayerInputPacketC2S> PopEarliestInputsOfEachPlayer()
        {
            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.Players[i];
                var playerId = playerState.Id;
                MatchPlayerInputPacketC2S earliestPlayerInput;
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

        private bool TryGetCachedInputForPlayer(ushort playerId, out MatchPlayerInputPacketC2S playerInputPacket)
        {
            return _lastProcessedInputPerPlayer.TryGetValue(playerId, out playerInputPacket);
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
            
            var playerId = (ushort)peer.Tag;
            var heighestProcessedTickOfPlayer = _heighestProcessedTickPerPlayer.TryGetValue(playerId, out int value) ? value : -1;
            if (newPacket.HeighestProcessedTickFromServer > heighestProcessedTickOfPlayer)
            {
                _heighestProcessedTickPerPlayer[playerId] = newPacket.HeighestProcessedTickFromServer;
            }
            OnPlayerInputReceived(newPacket, playerId);
        }

        private void OnPlayerInputReceived(MatchPlayerInputPacketC2S playerInputPacket, ushort playerId)
        {
            if (!_inputsPerPlayer.ContainsKey(playerId))
            {
                _inputsPerPlayer.Add(playerId, _inputsListsPool.Get());
            }
    
            var inputsList = _inputsPerPlayer[playerId];
            ref var input = ref inputsList.AddAndGet();
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
        public CapacityDict<ushort, MatchPlayerInputPacketC2S> EarliestInputsPerPlayer;
        public ProcessPlayersInputsResult(int maxConcurrentPlayers)
        {
            HeighestProcessedTickPerPlayer = new CapacityDict<ushort, int>(maxConcurrentPlayers);
            EarliestInputsPerPlayer = new CapacityDict<ushort, MatchPlayerInputPacketC2S>(maxConcurrentPlayers);
        }

        public void Clear()
        {
            HeighestProcessedTickPerPlayer.Clear();
            EarliestInputsPerPlayer.Clear();
        }
    }
}