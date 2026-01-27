using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.InitiatorInvokerService;
using CoreDomain.Scripts.Services.SceneService;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers
{
    public class StartMatchPacketsHandler : IStartMatchPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IInitiatorInvokerService _initiatorInvokerService;
        private readonly ConcurrentPool<MatchFullTickPacket> _fullTickPacketsPool;
        private readonly IMatchMakingDataService _matchMakingDataService;

        public PacketTypeS2C PacketType => PacketTypeS2C.MatchFullTick;

        public StartMatchPacketsHandler(IClientNetworkManager networkManager, NetworkConfig networkConfig, IInitiatorInvokerService initiatorInvokerService, SharedGamePlayConfig sharedGamePlayConfig, IMatchMakingDataService matchMakingDataService)
        {
            _networkManager = networkManager;
            _initiatorInvokerService = initiatorInvokerService;
            _matchMakingDataService = matchMakingDataService;
            _fullTickPacketsPool = new ConcurrentPool<MatchFullTickPacket>(() => new MatchFullTickPacket(networkConfig.MaxCap, sharedGamePlayConfig), networkConfig.MaxCap.FullTickPacketsNetEvents);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }

        public void OnPacketReceived(NetDataReader reader)
        {
            var packet = _fullTickPacketsPool.Get();
            packet.Deserialize(reader);

            if (!packet.StartMatchNetEvents.IsNullOrEmpty())
            {
                var state = packet.StartMatchNetEvents[0].InitialState;
                var enterData = new GamePlayMatchInitiatorEnterData
                {
                    InitialState = state,
                    LocalPlayerId = _matchMakingDataService.LocalPlayer.PlayerId
                };

                _initiatorInvokerService.SwitchScene(SceneType.GamePlayMatchScene, enterData);
            }

            _fullTickPacketsPool.Return(packet);
        }
    }
}
