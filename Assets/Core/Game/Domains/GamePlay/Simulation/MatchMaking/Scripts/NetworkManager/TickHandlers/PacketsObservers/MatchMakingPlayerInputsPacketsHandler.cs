using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
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

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public class MatchMakingPlayerInputsPacketsHandler : IPlayerInputsPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchMakingDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly NetworkConfig _networkConfig;
        private readonly IPlaybackRecorderService _playerbackRecorderService;
        private readonly ISimulationInputService _simulationInputService;

        private readonly CapacityDict<ushort, FixedUnorderedList<MatchMakingPlayersInputPacketC2S>> _inputsPerPlayer;
        private readonly CapacityDict<ushort, MatchMakingPlayersInputPacketC2S> _lastProcessedInputPerPlayer;
        private readonly ConcurrentPool<MatchMakingPlayersInputPacketC2S> _playerInputPacketsPool;
        private readonly ConcurrentPool<FixedUnorderedList<MatchMakingPlayersInputPacketC2S>> _inputsListsPool;
        private readonly CapacityDict<ushort, int> _heighestProcessedTickPerPlayer;

        private readonly ProcessPlayersInputsResult _cachedProcessPlayersInputsResult;

        public PacketTypeC2S PacketType => PacketTypeC2S.MatchMakingPlayersInput;

        public MatchMakingPlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchMakingDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator, INetEventsDataService netEventsDataService,
            NetworkConfig networkConfig, IPlaybackRecorderService playerbackRecorderService, ISimulationInputService simulationInputService, int inputPacketsSavedPerPlayer)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _networkConfig = networkConfig;
            _playerbackRecorderService = playerbackRecorderService;
            _simulationInputService = simulationInputService;

            _lastProcessedInputPerPlayer = new CapacityDict<ushort, MatchMakingPlayersInputPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<MatchMakingPlayersInputPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            _heighestProcessedTickPerPlayer = new CapacityDict<ushort, int>(networkConfig.MaxCap.ConcurrentPlayers);
            _inputsListsPool = new ConcurrentPool<FixedUnorderedList<MatchMakingPlayersInputPacketC2S>>(() => new FixedUnorderedList<MatchMakingPlayersInputPacketC2S>(inputPacketsSavedPerPlayer), networkConfig.MaxCap.ConcurrentPlayers);
            _playerInputPacketsPool = new ConcurrentPool<MatchMakingPlayersInputPacketC2S>(() => new MatchMakingPlayersInputPacketC2S(), networkConfig.MaxCap.ConcurrentInputsProcessed);

            _cachedProcessPlayersInputsResult = new ProcessPlayersInputsResult(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }

        public ProcessPlayersInputsResult ProcessInputs(int processedTick)
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

                UpdatePlayerDirection(playerInputPacket, playerState);
                UpdatePlayerShoot(processedTick, GetInputForPlayer(playerInputPacket, playerState.Id).IsShootInputPressed, playerState);

                if (_lastProcessedInputPerPlayer.TryGetValue(playerId, out var lastPlayerInput))
                {
                    _playerInputPacketsPool.Return(lastPlayerInput);
                }
                _lastProcessedInputPerPlayer[playerId] = playerInputPacket;
            }

            return _cachedProcessPlayersInputsResult;
        }

        private void RemoveAmountOfEarliestInputs(FixedUnorderedList<MatchMakingPlayersInputPacketC2S> inputsOfPlayer, int amountOfPacketsToRemove)
        {
            for (int i = 0; i < amountOfPacketsToRemove; i++)
            {
                var indexOfEarliestInput = GetIndexOfEarliestInput(inputsOfPlayer);
                var removedInput = inputsOfPlayer[indexOfEarliestInput];
                inputsOfPlayer.RemoveAt(indexOfEarliestInput);
                _playerInputPacketsPool.Return(removedInput);
            }
        }

        private CapacityDict<ushort, MatchMakingPlayersInputPacketC2S> ProcessEarliestInputPerPlayers(int processedTick)
        {
            var earliestInputPerPlayers = PopEarliestInputsOfEachPlayer();

            foreach (var kvp in earliestInputPerPlayers)
            {
                var playerId = kvp.Key;
                var playerInputPacket = kvp.Value;
                var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);

                UpdatePlayerDirection(playerInputPacket, playerState);
                UpdatePlayerShoot(processedTick, GetInputForPlayer(playerInputPacket, playerState.Id).IsShootInputPressed, playerState);

                if (_lastProcessedInputPerPlayer.TryGetValue(playerId, out var lastPlayerInput))
                {
                    _playerInputPacketsPool.Return(lastPlayerInput);
                }
                _lastProcessedInputPerPlayer[playerId] = playerInputPacket;
            }

            return earliestInputPerPlayers;
        }

        private void UpdatePlayerShoot(int processedTick, bool isShootInputPressed, MatchMakingPlayerStateS2C playerModel)
        {
            var playerId = playerModel.Id;
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.Shoot, isShootInputPressed);
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

        private void UpdatePlayerDirection(MatchMakingPlayersInputPacketC2S playersInputPacket, MatchMakingPlayerStateS2C playerModel)
        {
            var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (GetInputForPlayer(playersInputPacket, playerModel.Id).IsMoveLeftInputPressed.ToInt() -
                 GetInputForPlayer(playersInputPacket, playerModel.Id).IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerModel.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerModel.Spaceship.Transform.Direction = rotatedVector;
            if (GetInputForPlayer(playersInputPacket, playerModel.Id).IsMoveForwardInputPressed)
            {
                playerModel.Spaceship.Transform.Velocity = playerModel.Spaceship.Transform.Direction * _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.TargetMovementSpeed;
            }
            else
            {
                playerModel.Spaceship.Transform.Velocity = System.Numerics.Vector2.Zero;
            }
        }

        private int GetIndexOfEarliestInput(FixedUnorderedList<MatchMakingPlayersInputPacketC2S> inputs)
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
        
        private CapacityDict<ushort, MatchMakingPlayersInputPacketC2S> PopEarliestInputsOfEachPlayer()
        {
            for (var i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.Players[i];
                var playerId = playerState.Id;
                MatchMakingPlayersInputPacketC2S earliestPlayersInput;
                if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs) && playerInputs.Count > 0)
                {
                    if (playerInputs.Count == 0) continue;
var indexOfEarliestInput = GetIndexOfEarliestInput(playerInputs);
                    earliestPlayersInput = playerInputs[indexOfEarliestInput];
                    playerInputs.RemoveAt(indexOfEarliestInput);
                }
                else
                {
                    if (!TryGetCachedInputForPlayer(playerId, out earliestPlayersInput))
                    {
                        continue;
                    }
                }
                
                _cachedProcessPlayersInputsResult.EarliestInputsPerPlayer.Add(playerId, earliestPlayersInput);
            }

            return _cachedProcessPlayersInputsResult.EarliestInputsPerPlayer;
        }

        private bool TryGetCachedInputForPlayer(ushort playerId, out MatchMakingPlayersInputPacketC2S playersInputPacket)
        {
            return _lastProcessedInputPerPlayer.TryGetValue(playerId, out playersInputPacket);
        }
        
        public void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback)
        {
            var newPacket = _playerInputPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnPlayerInputReceived(newPacket, peer);
        }

        private void OnPlayerInputReceived(MatchMakingPlayersInputPacketC2S playersInputPacket, NetPeer peer)
        {
            if (playersInputPacket.Inputs == null) return;
            for(int _i=0; _i<playersInputPacket.Inputs.Length; _i++)
            {
                var inputData = playersInputPacket.Inputs[_i];
                var playerId = inputData.PlayerId;

                if (!_inputsPerPlayer.ContainsKey(playerId))
                {
                    _inputsPerPlayer.Add(playerId, _inputsListsPool.Get());
                }
                ref var input = ref _inputsPerPlayer[playerId].AddAndGet();
                input = new MatchMakingPlayersInputPacketC2S { Tick = playersInputPacket.Tick, HeighestProcessedTickFromServer = playersInputPacket.HeighestProcessedTickFromServer, Inputs = new MatchMakingPlayerInputData[] { inputData } };
            }
        }

        public void ManagedOnGUI()
        {
        }


        private MatchMakingPlayerInputData GetInputForPlayer(MatchMakingPlayersInputPacketC2S packet, ushort playerId)
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
        public void ManagedOnDrawGizmos()
        {
        }
    }
}
