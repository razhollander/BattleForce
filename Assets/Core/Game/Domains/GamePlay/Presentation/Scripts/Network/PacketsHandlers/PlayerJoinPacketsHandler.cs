using System;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class PlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly IPlayerControllers _playerControllers;
        private readonly ITickProcessor _tickProcessor;
        private readonly NetworkConfig _networkConfig;
        private readonly IClientPresentationTickProcessor _clientPresentationTickProcessor;

        public PlayerJoinPacketsHandler(IClientNetworkManager networkManager, IMatchDataService matchDataService,
            IPlayerControllers playerControllers, ITickProcessor tickProcessor, NetworkConfig networkConfig, IClientPresentationTickProcessor clientPresentationTickProcessor)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _playerControllers = playerControllers;
            _tickProcessor = tickProcessor;
            _networkConfig = networkConfig;
            _clientPresentationTickProcessor = clientPresentationTickProcessor;
        }

        public void RegisterListeners()
        {
            _networkManager.SubscribeNetSerializable<JoinAcceptPacketS2C, NetPeer>(OnJoinAccept);
        }

        public void InitExitPoint()
        {
            UnregisterListeners();
        }

        private void UnregisterListeners()
        {
            _networkManager.RemoveSubscription<JoinAcceptPacketS2C>();
        }

        private void OnJoinAccept(JoinAcceptPacketS2C joinPacketS2C, NetPeer _) // needed netPeer?
        {
            var playerId = joinPacketS2C.PlayerId;
            
            LogService.LogTopic("Join packet accepted received, player id: " + playerId, LogTopicType.ClientNetwork);
            var ticksPassedSinceServerSendPacket = (_networkManager.Ping / 1000f) / _networkConfig.DeltaTime;
            var tickWouldBeOnServerWhenReceiveMyPackets = (int)(ticksPassedSinceServerSendPacket*2) + joinPacketS2C.TickOnServer;
            _tickProcessor.SetTick(tickWouldBeOnServerWhenReceiveMyPackets);
            var playerModel = _matchDataService.AddPlayer(playerId, joinPacketS2C.PlayerName, joinPacketS2C.SpaceshipState);
            _matchDataService.SetLocalPlayer(playerModel);
            _playerControllers.CreatePlayer(playerModel.PlayerId);
            _clientPresentationTickProcessor.StartTick();
        }
    }
}