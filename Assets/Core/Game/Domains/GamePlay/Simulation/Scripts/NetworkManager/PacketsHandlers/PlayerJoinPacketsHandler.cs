using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers
{
    public class PlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly GamePlayConfig _gamePlayConfig;
        private readonly Dictionary<int, (JoinRequestPacketC2S, NetPeer)> _packetsPerTick;

        public PlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService, GamePlayConfig gamePlayConfig)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
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
                RotationVector = Vector2.One,
                Velocity = Vector2.One * _gamePlayConfig.PlayerSpaceship.MovementSpeed
            };
            var playerState = _matchDataService.AddPlayer(joinRequestPacket.UserName, playerTransform,
                _gamePlayConfig.PlayerSpaceship.StartHealth, _gamePlayConfig.PlayerSpaceship.ShootCooldown);
            var playerId = playerState.Id;
            peer.Tag = playerId;
            _networkManager.AddPlayerPeer(playerId, peer);
            _networkManager.SendPacketSerialized(PacketTypeS2C.JoinAccepted,
                new JoinAcceptPacketS2C
                {
                    PlayerId = playerId, PlayerName = playerState.Name, SpaceshipState = playerState.Spaceship
                },
                DeliveryMethod.ReliableOrdered);
        }
    }
}