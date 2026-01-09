using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsHandlers
{
    public class PlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly CapacityDict<NetPeer, JoinRequestPacketC2S> _playerJoinedPacketsPerPeer;
        private readonly ConcurrentPool<JoinRequestPacketC2S> _joinedRequestPacketsPool;
        public PacketTypeC2S PacketType => PacketTypeC2S.JoinRequest;

        public PlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, IPhysicsSimulator physicsSimulator,
            IMatchNetEventsDataService matchNetEventsDataService, NetworkConfig networkConfig)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _matchNetEventsDataService = matchNetEventsDataService;
            _playerJoinedPacketsPerPeer = new CapacityDict<NetPeer, JoinRequestPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _joinedRequestPacketsPool = new ConcurrentPool<JoinRequestPacketC2S>(() => new JoinRequestPacketC2S(), networkConfig.MaxCap.JoinRequestPackets);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void ProcessPlayersJoined(int processedTick)
        {
            var startingDirection = new Vector2(0, 1);
            var velocity = startingDirection * _gamePlayConfig.PlayerSpaceship.MovementSpeed;
            var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
            var health = _gamePlayConfig.PlayerSpaceship.StartHealth;
            var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
            var position = Vector2.One;
            foreach (var kvp in _playerJoinedPacketsPerPeer)
            { 
                var playerName = kvp.Value.UserName;
                var playersAmount =_matchDataService.SimulationState.Players.Count;
                var playerColor = _gamePlayConfig.PlayerSpaceship.PlayerColors[playersAmount % _gamePlayConfig.PlayerSpaceship.PlayerColors.Length];
                var playerState = _matchDataService.AddPlayer(playerName, position, startingDirection, velocity, radius, health, shootCooldown, playerColor);
                var playerId = playerState.Id;
                var peer = kvp.Key;
                peer.Tag = playerId;
                _physicsSimulator.AddPlayer(playerId, playerState.TeamId, position, startingDirection, radius);
                _networkManager.AddPlayerPeer(playerId, peer);
                _matchNetEventsDataService.StartSavingPlayerEvents(playerId);
                _matchNetEventsDataService.AddPlayerJoinAcceptedEvent(processedTick, playerState, _matchDataService.SimulationState);
                LogService.LogTopic("Processed player joined: " + playerState.ToJson(), LogTopicType.ServerNetwork);
            }

            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                _joinedRequestPacketsPool.Return(kvp.Value);
            }
            _playerJoinedPacketsPerPeer.Clear();
        }
        
        public void OnPacketReceived(NetPacketReader reader, NetPeer peer)
        {
            var newPacket = _joinedRequestPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnJoinReceived(newPacket, peer);
        }
        
        private void OnJoinReceived(JoinRequestPacketC2S joinRequestPacket, NetPeer peer)
        {
            LogService.LogTopic("Join packet received: " + joinRequestPacket.UserName, LogTopicType.ServerNetwork);
            _playerJoinedPacketsPerPeer.Add(peer, joinRequestPacket);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }
    }
}