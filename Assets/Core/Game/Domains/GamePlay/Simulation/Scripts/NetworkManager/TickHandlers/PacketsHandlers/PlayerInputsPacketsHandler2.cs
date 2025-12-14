using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsHandlers
{
    public class PlayerInputsPacketsHandler : IPlayerInputsPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;

        private readonly IMatchNetEventsDataService _matchNetEventsDataService;

        // private readonly Dictionary<int, Dictionary<int, PlayerInputPacketC2S>> _inputsByTick = new ();
        private readonly Dictionary<int, List<PlayerInputPacketC2S>> _inputsPerPlayer = new();
        private readonly Dictionary<int,PlayerInputPacketC2S> _cachedLastProcessedInput = new ();

        public PlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, IMatchNetEventsDataService matchNetEventsDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _matchNetEventsDataService = matchNetEventsDataService;
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<PlayerInputPacketC2S>(OnPlayerInputReceived);
        }

        public void InitExitPoint()
        {
            _networkManager.RemoveSubscription<PlayerInputPacketC2S>();
        }

        public Dictionary<int, PlayerInputPacketC2S> ProcessInputs(int processedTick)
        {
            var earliestInputPerPlayers = PopEarliestInputsOfEachPlayer();            
            for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var player = _matchDataService.SimulationState.Players[i];
                var playerId = player.Id;
                if (!earliestInputPerPlayers.ContainsKey(playerId))
                {
                    LogService.LogTopic($"Didn't find any last cached inputs for player {playerId}!", LogTopicType.ServerNetwork);
                    continue;
                }

                var playerInputPacket = earliestInputPerPlayers[playerId];
                var playerModel = _matchDataService.GetPlayer(playerId);
                UpdatePlayerTransform(playerInputPacket, ref playerModel);
                UpdatePlayerShoot(processedTick, playerInputPacket.IsShootInputPressed, ref playerModel);
                _matchDataService.SetPlayer(playerId, playerModel);
                _cachedLastProcessedInput[playerId] = playerInputPacket;
            }
            
            return earliestInputPerPlayers;
        }

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
                playerModel.Spaceship.Transform.Direction, _gamePlayConfig.PlayerBullet.MoveSpeed);
            _matchNetEventsDataService.AddBulletSpawnNetEvent(processedTick, bullet.Id, bullet.BelongToPlayerId, bullet.Position);

            LogService.LogTopic($"CreateBulletForPlayer {bullet.ToJson()}", LogTopicType.ServerNetwork);
        }

        private void UpdatePlayerTransform(PlayerInputPacketC2S playerInputPacket, ref PlayerStateS2C playerModel)
        {
            var rotationDelta = _gamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (playerInputPacket.IsMoveLeftInputPressed.ToInt() -
                 playerInputPacket.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerModel.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerModel.Spaceship.Transform.Direction = rotatedVector;
            playerModel.Spaceship.Transform.Position += playerModel.Spaceship.Transform.Direction *
                                                        _gamePlayConfig.PlayerSpaceship.MovementSpeed *
                                                        _networkConfig.DeltaTime;
        }

        private Dictionary<int, PlayerInputPacketC2S> PopEarliestInputsOfEachPlayer()
        {
            var earliestInputsPerPlayer = new Dictionary<int, PlayerInputPacketC2S>();

            for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
            {
                var playerState = _matchDataService.SimulationState.Players[i];
                var playerId = playerState.Id;
                PlayerInputPacketC2S earliestPlayerInput;
                if (_inputsPerPlayer.TryGetValue(playerId, out var playerInputs))
                {
                    playerInputs.Sort();
                    earliestPlayerInput = playerInputs[0];
                    _inputsPerPlayer[playerId].Remove(earliestPlayerInput);
                    if (_inputsPerPlayer[playerId].Count == 0)
                    {
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

                earliestInputsPerPlayer.Add(playerId, earliestPlayerInput);
            }

            return earliestInputsPerPlayer;
        }

        private bool TryGetCachedInputForPlayer(int playerId, out PlayerInputPacketC2S playerInputPacket)
        {
            return _cachedLastProcessedInput.TryGetValue(playerId, out playerInputPacket);
        }

        private void OnPlayerInputReceived(PlayerInputPacketC2S playerInputPacket, NetPeer peer)
        {
            var playerId = (ushort)peer.Tag;
            _inputsPerPlayer.TryAdd(playerId, new List<PlayerInputPacketC2S>());
            _inputsPerPlayer[playerId].Add(playerInputPacket);
            LogService.LogTopic("Input packet received from player id" + playerId, LogTopicType.ServerNetwork);
        }
    }
}