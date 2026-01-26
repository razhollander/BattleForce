using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers
{
    public class MatchMakingPlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchMakingDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly CapacityDict<NetPeer, JoinRequestPacketC2S> _playerJoinedPacketsPerPeer;
        private readonly ConcurrentPool<JoinRequestPacketC2S> _joinedRequestPacketsPool;
        public PacketTypeC2S PacketType => PacketTypeC2S.MatchMakingJoinRequest;

        public MatchMakingPlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchMakingDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, IPhysicsSimulator physicsSimulator,
            INetEventsDataService iNetEventsDataService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = iNetEventsDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _playerJoinedPacketsPerPeer = new CapacityDict<NetPeer, JoinRequestPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _joinedRequestPacketsPool = new ConcurrentPool<JoinRequestPacketC2S>(() => new JoinRequestPacketC2S(), networkConfig.MaxCap.JoinRequestPackets);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void ProcessPlayersJoined(int processedTick)
        {
            var startingDirection = Vector2.One;//RNG.NextFloat(0, 360).AngleToVector();
            var velocity = startingDirection * _gamePlayConfig.PlayerSpaceship.MovementSpeed;
            var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
            var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
            var position = Vector2.One;

            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                var playerName = kvp.Value.UserName;
                var playersAmount = _matchDataService.SimulationState.Players.Count;
                var playerColor = _sharedGamePlayConfig.ColorPerTeamId[playersAmount % _sharedGamePlayConfig.ColorPerTeamId.Count];
                var playerState = _matchDataService.AddPlayer(playerName, position, startingDirection, velocity, radius, shootCooldown, playerColor);
                var playerId = playerState.Id;
                var peer = kvp.Key;
                peer.Tag = playerId;
                _physicsSimulator.AddPlayer(playerId, playerState.TeamId, position, startingDirection, radius);
                _networkManager.AddPlayerPeer(playerId, peer);
                _netEventsDataService.StartSavingPlayerEvents(playerId);
                _netEventsDataService.AddMatchMakingPlayerJoinAcceptedEvent(processedTick, playerState, _matchDataService.SimulationState);
                LogService.LogTopic("Processed player joined: " + playerState.ToJson(), LogTopicType.ServerNetwork);
            }

            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                _joinedRequestPacketsPool.Return(kvp.Value);
            }

            _playerJoinedPacketsPerPeer.Clear();
        }

        public void OnPacketReceived(NetDataReader reader, NetPeer peer)
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