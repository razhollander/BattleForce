using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class NetworkManager : INetworkManager
    {
        private NetworkPacketsListener _networkPacketsListener;
        private NetManager _netManager;
        private NetPacketProcessor _packetProcessor;
        private NetDataWriter _cachedWriter;
        private readonly NetworkConfig _networkConfig;

        public NetworkManager(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            _networkPacketsListener = new NetworkPacketsListener(_networkConfig.TicksPerSeconds);
            _cachedWriter = new NetDataWriter();
            _netManager = new NetManager(_networkPacketsListener) { AutoRecycle = true };
            _packetProcessor = new NetPacketProcessor();
        }

        public void InitEntryPoint()
        {
            StartServer();
        }
        
        private void StartServer()
        {
            if (_netManager.IsRunning)
                return;
            _netManager.Start(_networkConfig.Port);
            _networkPacketsListener.InitializeEntryPoint();
        }
    }

    public interface INetworkManager
    {
    }
}