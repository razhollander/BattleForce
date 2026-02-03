using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers
{
    public interface IMatchStartMatchPacketHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
    }

    public class MatchStartMatchPacketHandler : IMatchStartMatchPacketHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly ICommandFactory _commandFactory;
        private readonly IMatchDataService _matchDataService;
        private StartMatchPacketS2C _startMatchPacket;

        public PacketTypeS2C PacketType => PacketTypeS2C.StartMatch;

        public MatchStartMatchPacketHandler(IClientNetworkManager networkManager, ICommandFactory commandFactory, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, IMatchDataService matchDataService)
        {
            _networkManager = networkManager;
            _commandFactory = commandFactory;
            _matchDataService = matchDataService;
            _startMatchPacket = new StartMatchPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
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
            int localPlayerId = -1;
            if (_matchDataService.LocalPlayer != null)
            {
                localPlayerId = _matchDataService.LocalPlayer.PlayerId;
            }

            _startMatchPacket.Deserialize(reader);
            LogService.LogTopic("Match Restart (StartMatch) received", LogTopicType.ClientNetwork);

            _commandFactory.CreateCommandVoid<SyncMatchSimulationStateCommand>()
                .SetSimulationState(_startMatchPacket.InitialState)
                .Execute();

            if (localPlayerId != -1)
            {
                _matchDataService.SetLocalPlayer(localPlayerId);
            }
        }
    }
}
