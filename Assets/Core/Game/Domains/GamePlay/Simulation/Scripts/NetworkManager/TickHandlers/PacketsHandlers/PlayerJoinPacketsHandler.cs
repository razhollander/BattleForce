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
        private readonly ITickProcessor _tickProcessor;
        private readonly Dictionary<int, (JoinRequestPacketC2S, NetPeer)> _packetsPerTick;
        private readonly IPlayerInputsPacketsHandler _playerInputsPacketsHandler;

        public PlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, ITickProcessor tickProcessor, IPlayerInputsPacketsHandler playerInputsPacketsHandler)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _tickProcessor = tickProcessor;
            _playerInputsPacketsHandler = playerInputsPacketsHandler;
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<JoinRequestPacketC2S>(OnJoinReceived);
        }

        public void InitExitPoint()
        {
            _networkManager.RemoveSubscription<JoinRequestPacketC2S>();
        }

        private void OnJoinReceived(JoinRequestPacketC2S joinRequestPacket, NetPeer peer)
        {
            LogService.LogTopic("Join packet received: " + joinRequestPacket.UserName, LogTopicType.ServerNetwork);
            var playerTransform = new PlayerTransformStateS2C
            {
                Acceleration = Vector2.Zero,
                AimVector = Vector2.Zero,
                AngularVelocity = 0,
                Position = Vector2.One,
                Direction = new Vector2(0,1),
                Velocity = Vector2.One * _gamePlayConfig.PlayerSpaceship.MovementSpeed,
                Radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius
            };
            var playerState = _matchDataService.AddPlayer(joinRequestPacket.UserName, playerTransform,
                _gamePlayConfig.PlayerSpaceship.StartHealth, _gamePlayConfig.PlayerSpaceship.ShootCooldown);

            // Log everything in playerState
            LogService.LogTopic("PlayerState: " + playerState.Spaceship.ToJson(), LogTopicType.ServerNetwork);

            var playerId = playerState.Id;
            peer.Tag = playerId;
            _playerInputsPacketsHandler.RegisterListeners();
            _networkManager.AddPlayerPeer(playerId, peer);
            var joinPacket = new JoinAcceptPacketS2C
            {
                TickOnServer = _tickProcessor.CurrentTick,
                PlayerId = playerId,
                PlayerName = playerState.Name,
                SpaceshipState = playerState.Spaceship
            };
            _networkManager.SendPacketSerialized(PacketTypeS2C.JoinAccepted, joinPacket, DeliveryMethod.ReliableOrdered);
        }
    }
}