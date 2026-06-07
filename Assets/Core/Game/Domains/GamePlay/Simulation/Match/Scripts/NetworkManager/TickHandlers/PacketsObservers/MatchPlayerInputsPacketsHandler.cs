using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
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
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public class MatchPlayerInputsPacketsHandler : IPlayerInputsPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly NetworkConfig _networkConfig;
        private readonly IPlaybackRecorderService _playerbackRecorderService;
        private readonly ISimulationInputService _simulationInputService;
        private readonly PlayersTalentsManager _playersTalentsManager;

        private readonly CapacityDict<ushort, FixedUnorderedList<MatchPlayerInputPacketC2S>> _inputsPerPlayer;
        private readonly CapacityDict<ushort, MatchPlayerInputPacketC2S> _lastProcessedInputPerPlayer;
        private readonly ConcurrentPool<MatchPlayerInputPacketC2S> _playerInputPacketsPool;
        private readonly ConcurrentPool<FixedUnorderedList<MatchPlayerInputPacketC2S>> _inputsListsPool;
        private readonly CapacityDict<ushort, int> _heighestProcessedTickPerPlayer;

        private readonly ProcessPlayersInputsResult _cachedProcessPlayersInputsResult;
        private readonly TryPerformShootForPlayerIfNotOnCooldownCommand _tryPerformShootForPlayerIfNotOnCooldownCommand;

        public PacketTypeC2S PacketType => PacketTypeC2S.MatchPlayerInput;

        public MatchPlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator, INetEventsDataService netEventsDataService,
            NetworkConfig networkConfig, IPlaybackRecorderService playerbackRecorderService, ISimulationInputService simulationInputService,
            int inputPacketsSavedPerPlayer, ICommandFactory commandFactory, PlayersTalentsManager playersTalentsManager)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _networkConfig = networkConfig;
            _playerbackRecorderService = playerbackRecorderService;
            _simulationInputService = simulationInputService;
            _playersTalentsManager = playersTalentsManager;

            _lastProcessedInputPerPlayer = new CapacityDict<ushort, MatchPlayerInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<MatchPlayerInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            _heighestProcessedTickPerPlayer = new CapacityDict<ushort, int>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsListsPool = new ConcurrentPool<FixedUnorderedList<MatchPlayerInputPacketC2S>>(() => new FixedUnorderedList<MatchPlayerInputPacketC2S>(inputPacketsSavedPerPlayer), networkConfig.MaxCap.ConcurrentPlayers);
            _playerInputPacketsPool = new ConcurrentPool<MatchPlayerInputPacketC2S>(() => new MatchPlayerInputPacketC2S(), networkConfig.MaxCap.ConcurrentInputsProcessed);

            _cachedProcessPlayersInputsResult = new ProcessPlayersInputsResult(networkConfig.MaxCap.ConcurrentPlayers);
            _tryPerformShootForPlayerIfNotOnCooldownCommand = commandFactory.CreateCommandVoid<TryPerformShootForPlayerIfNotOnCooldownCommand>();
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }

        public ProcessPlayersInputsResult ProcessInputs(int processedTick, float deltaTime)
        {
            _cachedProcessPlayersInputsResult.HeighestProcessedTickPerPlayer.Clear();
            _cachedProcessPlayersInputsResult.EarliestInputsPerPlayer.Clear();

            var earliestInputPerPlayers = PopEarliestInputsOfEachPlayer();

            foreach (var kvp in earliestInputPerPlayers)
            {
                var playerId = kvp.Key;
                var playerInputPacket = kvp.Value;
                var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
                if (playerState == null) continue;

                var isTalentAInputPressed = false;
                var isTalentBInputPressed = false;
                var isTalentCInputPressed = false;

                playerState.Spaceship.TalentsState.AimDirection = GetInputForPlayer(playerInputPacket, playerState.Id).AimDirection;
                UpdatePlayerDirection(playerInputPacket, playerState);
                isTalentAInputPressed = GetInputForPlayer(playerInputPacket, playerState.Id).IsTalentAInputPressed;
                isTalentBInputPressed = GetInputForPlayer(playerInputPacket, playerState.Id).IsTalentBInputPressed;
                isTalentCInputPressed = GetInputForPlayer(playerInputPacket, playerState.Id).IsTalentCInputPressed;

                if (_lastProcessedInputPerPlayer.TryGetValue(playerId, out var lastPlayerInput))
                {
                    _playerInputPacketsPool.Return(lastPlayerInput);
                }
                _lastProcessedInputPerPlayer[playerId] = playerInputPacket;
                
                _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentAInput, isTalentAInputPressed);
                _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentBInput, isTalentBInputPressed);
                _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentCInput, isTalentCInputPressed);
                
                TrySwitchTalent(processedTick, playerState);
                ProcessPlayerTalentInput(processedTick, isTalentAInputPressed, isTalentBInputPressed, isTalentCInputPressed, playerState, deltaTime);
            }

            return _cachedProcessPlayersInputsResult;
        }

        private void TrySwitchTalent(int processedTick, PlayerStateS2C playerState)
        {
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

        private void ProcessPlayerTalentInput(int processedTick, bool isTalentAInputPressed, bool isTalentBInputPressed, bool isTalentCInputPressed, PlayerStateS2C playerState, float deltaTime)
        {
            var isAnyInputPressed = isTalentAInputPressed || isTalentBInputPressed || isTalentCInputPressed;

            if (!playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) ||
                playerState.Spaceship.TalentsState.StocksLeft == 0 ||
                selectedTalent.CooldownSecondsLeft > 0 ||
                !isAnyInputPressed)
            {
                return;
            }

            _playersTalentsManager.TryPerformCurrentSelectedTalentForPlayer(playerState.Id, processedTick, deltaTime);
        }

        private void UpdatePlayerDirection(MatchPlayerInputPacketC2S playerInputPacket, PlayerStateS2C playerState)
        {
            if (playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) && selectedTalent.IsCurrentlyAiming)
            {
                return;
            }

            var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (GetInputForPlayer(playerInputPacket, playerState.Id).IsMoveLeftInputPressed.ToInt() -
                 GetInputForPlayer(playerInputPacket, playerState.Id).IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerState.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerState.Spaceship.Transform.Direction = rotatedVector;
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
        
        private CapacityDict<ushort, MatchPlayerInputPacketC2S> PopEarliestInputsOfEachPlayer()
        {
            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.Players[i];
                var playerId = playerState.Id;
                MatchPlayerInputPacketC2S earliestPlayerInput;
                if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs) && playerInputs.Count > 0)
                {
                    if (playerInputs.Count == 0) continue;
var indexOfEarliestInput = GetIndexOfEarliestInput(playerInputs);
                    earliestPlayerInput = playerInputs[indexOfEarliestInput];
                    playerInputs.RemoveAt(indexOfEarliestInput);
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
            OnPlayerInputReceived(newPacket, peer);
        }

        private void OnPlayerInputReceived(MatchPlayerInputPacketC2S playerInputPacket, NetPeer peer)
        {
            if (playerInputPacket.Inputs == null) return;
            for(int _i=0; _i<playerInputPacket.Inputs.Length; _i++)
            {
                var inputData = playerInputPacket.Inputs[_i];
                var playerId = inputData.PlayerId;

                if (!_inputsPerPlayer.ContainsKey(playerId))
                {
                    _inputsPerPlayer.Add(playerId, _inputsListsPool.Get());
                }
                ref var input = ref _inputsPerPlayer[playerId].AddAndGet();
                input = new MatchPlayerInputPacketC2S { Tick = playerInputPacket.Tick, HeighestProcessedTickFromServer = playerInputPacket.HeighestProcessedTickFromServer, Inputs = new PlayerInputData[] { inputData } };
            }
        }


        private PlayerInputData GetInputForPlayer(MatchPlayerInputPacketC2S packet, ushort playerId)
        {
            if (packet.Inputs != null)
            {
                for (int i = 0; i < packet.Inputs.Length; i++)
                {
                    if (packet.Inputs[i].PlayerId == playerId) return packet.Inputs[i];
                }
            }
            return default;
        }
        public void ManagedOnGUI()
        {
        }

        public void ManagedOnDrawGizmos()
        {
        }
    }
}
