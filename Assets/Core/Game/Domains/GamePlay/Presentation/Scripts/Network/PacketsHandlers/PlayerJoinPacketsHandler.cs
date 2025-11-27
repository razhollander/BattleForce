using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class PlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly IPlayerControllers _playerControllers;

        public PlayerJoinPacketsHandler(IClientNetworkManager networkManager, IMatchDataService matchDataService, IPlayerControllers playerControllers)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _playerControllers = playerControllers;
        }

        public void RegisterListeners()
        {
            _networkManager.SubscribeNetSerializable<JoinAcceptPacketS2C, NetPeer>(OnJoinAccept);
        }

        public void UnregisterListeners()
        {
            _networkManager.RemoveSubscription<JoinAcceptPacketS2C>();
        }

        public void InitExitPoint()
        {
            UnregisterListeners();
        }

        private void OnJoinAccept(JoinAcceptPacketS2C joinPacketS2C, NetPeer _) // needed netPeer?
        {
            LogService.LogTopic("Join packet accepted received, player id: " + joinPacketS2C.PlayerId, LogTopicType.ClientNetwork);
            var playerModel = _matchDataService.AddPlayer(joinPacketS2C.PlayerId, joinPacketS2C.PlayerName, joinPacketS2C.PlayerTransform);
            _matchDataService.SetLocalPlayer(playerModel);
            _playerControllers.CreatePlayer(playerModel.PlayerId);
        }
    }
}