using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers;
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
        private readonly Dictionary<int, (JoinRequestPacketC2S, NetPeer)> _packetsPerTick;
        private readonly IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly Dictionary<NetPeer, JoinRequestPacketC2S> _playerJoinedPacketsPerPeer;

        public PlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            SimulationGamePlayConfig gamePlayConfig, IPlayerInputsPacketsHandler playerInputsPacketsHandler,
            IMatchNetEventsDataService matchNetEventsDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
            _matchNetEventsDataService = matchNetEventsDataService;
            _playerJoinedPacketsPerPeer = new Dictionary<NetPeer, JoinRequestPacketC2S>();
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<JoinRequestPacketC2S>(OnJoinReceived);
        }

        public void ProcessPlayersJoined(int processedTick)
        {
            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                var playerTransform = new PlayerTransformStateS2C
                {
                    Acceleration = Vector2.Zero,
                    AimVector = Vector2.Zero,
                    AngularVelocity = 0,
                    Position = Vector2.One,
                    Direction = new Vector2(0, 1),
                    Velocity = Vector2.One * _gamePlayConfig.PlayerSpaceship.MovementSpeed,
                    Radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius
                };

                var playerName = kvp.Value.UserName;
                var playerState = _matchDataService.AddPlayer(playerName, playerTransform,
                    _gamePlayConfig.PlayerSpaceship.StartHealth, _gamePlayConfig.PlayerSpaceship.ShootCooldown);
                var playerId = playerState.Id;
                var peer = kvp.Key;
                peer.Tag = playerId;
                _networkManager.AddPlayerPeer(playerId, peer);
                _matchNetEventsDataService.StartSavingPlayerEvents(playerId);
                _matchNetEventsDataService.AddPlayerJoinAcceptedEvent(processedTick, _networkManager.GetPlayerPeerId(playerId), playerName, playerState.Spaceship, playerId);
                LogService.LogTopic("Processed player joined: " + playerState.ToJson(), LogTopicType.ServerNetwork);
            }
            
            _playerJoinedPacketsPerPeer.Clear();
        }

        private void OnJoinReceived(JoinRequestPacketC2S joinRequestPacket, NetPeer peer)
        {
            LogService.LogTopic("Join packet received: " + joinRequestPacket.UserName, LogTopicType.ServerNetwork);
            _playerJoinedPacketsPerPeer.Add(peer, joinRequestPacket);
        }

        public void InitExitPoint()
        {
            _networkManager.RemoveSubscription<JoinRequestPacketC2S>();
        }
    }
}