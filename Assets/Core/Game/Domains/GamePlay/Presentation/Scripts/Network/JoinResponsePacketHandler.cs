using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public interface IJoinResponsePacketHandler
    {
        bool DidReceiveJoinResponse { get; }
        JoinResponsePacketS2C JoinResponse { get; }
        void InitEntryPoint();
        void InitExitPoint();
        void Reset();
    }

    public class JoinResponsePacketHandler : IPacketsObserver, IJoinResponsePacketHandler
    {
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly IClientNetworkManager _networkManager;
        
        public bool DidReceiveJoinResponse => JoinResponse != null;
        public JoinResponsePacketS2C JoinResponse { get; private set; }

        public JoinResponsePacketHandler(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, IClientNetworkManager networkManager)
        {
            _networkConfig = networkConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkManager = networkManager;
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }
        
        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }

        public void Reset()
        {
            JoinResponse = null;
        }

        public PacketTypeS2C PacketType => PacketTypeS2C.JoinResponse;
        
        public void OnPacketReceived(NetDataReader reader)
        {
            JoinResponse = new JoinResponsePacketS2C(_networkConfig.MaxCap, _sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
            JoinResponse.Deserialize(reader);
        }
    }
}