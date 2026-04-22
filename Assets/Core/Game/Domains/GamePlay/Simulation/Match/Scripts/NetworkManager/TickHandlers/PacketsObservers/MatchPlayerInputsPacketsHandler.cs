using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
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
        private readonly SimulationGamePlayConfig _gamePlayConfig;
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
            SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, INetEventsDataService iNetEventsDataService, IPhysicsSimulator physicsSimulator, IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IPlayersTalentsManager playersTalentsManager, IPlaybackRecorderService playerbackRecorderService, ISimulationInputService simulationInputService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _netEventsDataService = iNetEventsDataService;
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
                // UpdatePlayerShoot(processedTick, true, playerState);
                var isTalentInputPressed = false;
                if (earliestInputPerPlayers.TryGetValue(playerId, out var playerInputPacket))
                {
                    playerState.Spaceship.TalentsState.AimDirection = playerInputPacket.AimDirection;
                    UpdatePlayerShoot(processedTick, playerInputPacket.IsShootInputPressed, playerState);
                    UpdatePlayerDirection(playerInputPacket, playerState);
                    ProcessPlayerSwitchTalentInput(processedTick, playerId, playerInputPacket, playerState);
                    isTalentInputPressed = playerInputPacket.IsTalentInputPressed;
                    
                    if (_lastProcessedInputPerPlayer.TryGetValue(playerId, out var lastPlayerInput))
                    {
                        _playerInputPacketsPool.Return(lastPlayerInput);
                    }
                    _lastProcessedInputPerPlayer[playerId] = playerInputPacket;
                }
                
                ProcessPlayerTalentInput(processedTick, isTalentInputPressed, playerState, deltaTime);
            }

            return earliestInputPerPlayers;
        }

        private void ProcessPlayerSwitchTalentInput(int processedTick, ushort playerId, MatchPlayerInputPacketC2S playerInputPacket, PlayerStateS2C playerState)
        {
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.SwitchTalent, playerInputPacket.IsSwitchTalentInputPressed);

            if (_simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.SwitchTalent))
            {
                if (_playersTalentsManager.TrySwitchToNextTalent(playerId))
                {
                    _netEventsDataService.AddTalentSwitchNetEvent(processedTick, playerId, playerState.Spaceship.TalentsState.SelectedTalentIndex);
                }
            }
        }

        private void ProcessPlayerTalentInput(int processedTick, bool isTalentInputPressed, PlayerStateS2C playerState, float deltaTime)
        {
            if (!playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalent))
            {
                return;
            }

            var playerId = playerState.Id;
            _playersTalentsManager.ProcessAllTalentsTickOfPlayer(playerId, processedTick, deltaTime);

            if (currentSelectedTalent.IsOnCooldown())
            {
                return;
            }

            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentInput, isTalentInputPressed);

            var wasTalentInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.TalentInput);
            _playersTalentsManager.ProcessPlayerTalentInput(playerId, currentSelectedTalent.TalentType, processedTick, wasTalentInputDownThisTick, isTalentInputPressed, deltaTime);
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
            var wasShootInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.Shoot);
            // var wasShootInputDownThisTick = true;
            if (wasShootInputDownThisTick)
            {
                _tryPerformShootForPlayerIfNotOnCooldownCommand.SetPlayerId(playerId).SetTick(processedTick).Execute();
                
                //
                // var rectSize = new System.Numerics.Vector2(10, 5);
                // var rectDistanceFromPlayer = playerModel.Spaceship.Transform.Radius + rectSize.X / 2;
                // var center = playerModel.Spaceship.Transform.Position+rectDistanceFromPlayer*playerModel.Spaceship.TalentsState.AimDirection;
                //
                // float angleRadians = playerModel.Spaceship.TalentsState.AimDirection.ToAngleRadians();
                // if (_physicsSimulator.RectangleCast(center, rectSize, angleRadians,
                //         PhysicsBodyType.Wall))
                // {
                //     LogService.LogError("Hit!");
                // }
            }
        }

        private void UpdatePlayerDirection(MatchPlayerInputPacketC2S playerInputPacket, PlayerStateS2C playerState)
        {
            var rotationDelta = _gamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
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
            if (newPacket.Tick > heighestProcessedTickOfPlayer)
            {
                _heighestProcessedTickPerPlayer[playerId] = newPacket.Tick;
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