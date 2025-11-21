using Core.Game.Domains.GamePlay.Simulation;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class NetworkManager : INetworkManager
    {
        private NetworkC2SPacketsListener _networkC2SPacketsListener;
        private NetManager _netManager;
        private NetPacketProcessor _packetProcessor;
        private NetDataWriter _cachedWriter;
        private readonly NetworkConfig _networkConfig;
        private readonly IStateMachineService _stateMachineService;
        private readonly NetworkTickProcessor _networkTickProcessor;
        private readonly ServerPlayersInputListener _serverPlayersInputListener;

        public NetworkManager(NetworkConfig networkConfig, IStateMachineService stateMachineService)
        {
            _networkConfig = networkConfig;
            _stateMachineService = stateMachineService;
            _networkC2SPacketsListener = new NetworkC2SPacketsListener();
            _serverPlayersInputListener = new ServerPlayersInputListener(_networkC2SPacketsListener);
            _cachedWriter = new NetDataWriter();
            _netManager = new NetManager(_networkC2SPacketsListener) { AutoRecycle = true };
            _packetProcessor = new NetPacketProcessor();
            _networkTickProcessor = new NetworkTickProcessor(_networkC2SPacketsListener, _serverPlayersInputListener); 
        }

        public void InitEntryPoint()
        {
            StartServer();
        }
        
        private void StartServer()
        {
            if (_netManager.IsRunning)
            {
                LogService.LogError("Server already running!");
                return;
            }
            
            _netManager.Start(_networkConfig.Port);
            _networkTickProcessor.StartTick(_networkConfig.TicksPerSeconds, _stateMachineService.CurrentState().CancellationTokenSource);
        }
        
        public void InitExitPoint()
        {
            _netManager.Stop();
            _networkTickProcessor.StopTick();
        }
    }
}