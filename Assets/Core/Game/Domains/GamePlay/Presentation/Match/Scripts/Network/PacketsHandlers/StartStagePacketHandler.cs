using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers
{
    public class StartStagePacketHandler : IStartStagePacketHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly ICommandFactory _commandFactory;
        private readonly StartStagePacketS2C _startStagePacket;

        public PacketTypeS2C PacketType => PacketTypeS2C.StartStage;

        public StartStagePacketHandler(IClientNetworkManager networkManager, ICommandFactory commandFactory, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _networkManager = networkManager;
            _commandFactory = commandFactory;
            _startStagePacket = new StartStagePacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
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
            _startStagePacket.Deserialize(reader);
            LogService.LogTopic("Stage start received", LogTopicType.ClientNetwork);
            _commandFactory.CreateCommandVoid<SyncMatchSimulationStateCommand>()
                .SetSimulationState(_startStagePacket.InitialState)
                .Execute();
            LogService.LogError("Stage start received");
        }
    }
}
