using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Extensions;
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
        private readonly Dictionary<int, (JoinRequestPacketC2S, NetPeer)> _packetsPerTick;
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly Dictionary<NetPeer, JoinRequestPacketC2S> _playerJoinedPacketsPerPeer;

        public PlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, IPhysicsSimulator physicsSimulator,
            IMatchNetEventsDataService matchNetEventsDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _matchNetEventsDataService = matchNetEventsDataService;
            _playerJoinedPacketsPerPeer = new Dictionary<NetPeer, JoinRequestPacketC2S>();
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<JoinRequestPacketC2S>(OnJoinReceived);
        }

        public void ProcessPlayersJoined(int processedTick)
        {
            var startingDirection = new Vector2(0, 1);
            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                var playerTransform = new PlayerTransformStateS2C
                {
                    Acceleration = Vector2.Zero,
                    AimVector = Vector2.Zero,
                    AngularVelocity = 0,
                    Position = Vector2.One,
                    Direction = startingDirection,
                    Velocity = startingDirection * _gamePlayConfig.PlayerSpaceship.MovementSpeed,
                    Radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius
                };

                var playerName = kvp.Value.UserName;
                var playerState = _matchDataService.AddPlayer(playerName, playerTransform,
                    _gamePlayConfig.PlayerSpaceship.StartHealth, _gamePlayConfig.PlayerSpaceship.ShootCooldown);
                var playerId = playerState.Id;
                var peer = kvp.Key;
                peer.Tag = playerId;
                _physicsSimulator.AddPlayer(playerId, playerState.TeamId, playerTransform.Position, playerTransform.Direction, playerTransform.Radius);
                _networkManager.AddPlayerPeer(playerId, peer);
                _matchNetEventsDataService.StartSavingPlayerEvents(playerId);
                _matchNetEventsDataService.AddPlayerJoinAcceptedEvent(processedTick, playerState, _matchDataService.SimulationState);
#if Logs
                LogService.LogTopic("Processed player joined: " + playerState.ToJson(), LogTopicType.ServerNetwork);
#endif
            }
            
            _playerJoinedPacketsPerPeer.Clear();
        }

        private void OnJoinReceived(JoinRequestPacketC2S joinRequestPacket, NetPeer peer)
        {
#if Logs
            LogService.LogTopic("Join packet received: " + joinRequestPacket.UserName, LogTopicType.ServerNetwork);
#endif
            _playerJoinedPacketsPerPeer.Add(peer, joinRequestPacket);
        }

        public void InitExitPoint()
        {
            _networkManager.RemoveSubscription<JoinRequestPacketC2S>();
        }
    }
}