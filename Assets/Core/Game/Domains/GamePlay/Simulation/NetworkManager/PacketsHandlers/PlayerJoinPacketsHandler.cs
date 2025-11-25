using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
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

        public PlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService, GamePlayConfig gamePlayConfig)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<JoinRequestPacketC2S, NetPeer>(OnJoinReceived);
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
                CurrentAcceleration = Vector2.Zero,
                CurrentAimAngle = 0,
                CurrentAngularVelocity = 0,
                CurrentPosition = Vector2.One,
                CurrentRotationAngle = 90,
                CurrentVelocity = Vector2.One * _gamePlayConfig.PlayerSpaceship.MovementSpeed
            };
            var playerModel = _matchDataService.AddPlayer(joinRequestPacket.UserName, playerTransform);
            var playerId = playerModel.PlayerId;
            peer.Tag = playerId;
            _networkManager.AddPlayerPeer(playerId, peer);
            _networkManager.SendPacketSerialized(PacketTypeS2C.JoinAccepted,
                new JoinAcceptPacketS2C
                {
                    PlayerId = playerId, PlayerName = playerModel.PlayerName, PlayerTransform = playerTransform
                },
                DeliveryMethod.ReliableOrdered);
        }
    }
}