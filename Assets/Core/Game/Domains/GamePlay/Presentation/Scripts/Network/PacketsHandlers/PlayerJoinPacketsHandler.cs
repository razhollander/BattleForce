using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers
{
    public class PlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;

        public PlayerJoinPacketsHandler(IClientNetworkManager networkManager, IMatchDataService matchDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<JoinAcceptPacketS2C, NetPeer>(OnJoinAccept);
        }

        public void InitExitPoint()
        {
            _networkManager.RemoveSubscription<JoinAcceptPacketS2C>();
        }

        private void OnJoinAccept(JoinAcceptPacketS2C joinPacketS2C, NetPeer _) // needed netPeer?
        {
            LogService.LogTopic("Join packet accepted received, player id: " + joinPacketS2C.PlayerId, LogTopicType.ClientNetwork);
            var playerModel = _matchDataService.AddPlayer(joinPacketS2C.PlayerId, joinPacketS2C.PlayerName);
            _matchDataService.SetLocalPlayer(playerModel);
        }
    }
}