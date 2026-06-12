using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Services.UnityThreadDispatcher;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers
{
    public class HeadLessQuitterController : IHeadLessQuitterController
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly NetworkConfig _networkConfig;
        private readonly IUnityMainThreadDispatcher _unityMainThreadDispatcher;

        private float _timePassedSinceLastPacket;

        public HeadLessQuitterController(IServerNetworkManager networkManager, NetworkConfig networkConfig, IUnityMainThreadDispatcher unityMainThreadDispatcher)
        {
            _networkManager = networkManager;
            _networkConfig = networkConfig;
            _unityMainThreadDispatcher = unityMainThreadDispatcher;
        }

        public void InitEntryPoint()
        {
            if (!IsHeadless())
            {
                return;
            }

            _timePassedSinceLastPacket = 0;
            _networkManager.OnPacketReceivedEvent += OnPacketReceived;
            _networkManager.OnClientPeerDisconnectedEvent += OnPeerDisconnected;
            LogService.LogTopic("HeadLessQuitterController Initialized", LogTopicType.ServerNetwork);
        }

        public void InitExitPoint()
        {
            if (!IsHeadless())
            {
                return;
            }

            _networkManager.OnPacketReceivedEvent -= OnPacketReceived;
            _networkManager.OnClientPeerDisconnectedEvent -= OnPeerDisconnected;
        }

        public void StepTimer(float deltaTime)
        {
            if (!IsHeadless())
            {
                return;
            }
            
            _timePassedSinceLastPacket += deltaTime;
        }

        public void QuitIfTimeOut()
        {
            if (!IsHeadless())
            {
                return;
            }
            
            var didReachTimeOot = _timePassedSinceLastPacket > _networkConfig.HeadlessQuitTimeoutSeconds;
            if (didReachTimeOot)
            {
                LogService.LogTopic($"No packets received for {_networkConfig.HeadlessQuitTimeoutSeconds} seconds. Quitting...", LogTopicType.ServerNetwork);
                QuitApplication();
            }
        }

        private void OnPacketReceived()
        {
            _timePassedSinceLastPacket = 0;
        }

        private void OnPeerDisconnected(long clientId)
        {
            if (_networkManager.ConnectedPeersCount == 0)
            {
                LogService.LogTopic("[HeadLessQuitterController] All players disconnected. Quitting...", LogTopicType.ServerNetwork);
                QuitApplication();
            }
        }

        private void QuitApplication()
        {
#if UNITY_EDITOR
            _unityMainThreadDispatcher.Enqueue(() => UnityEditor.EditorApplication.isPlaying = false);
#else
            _unityMainThreadDispatcher.Enqueue(UnityEngine.Application.Quit);
#endif
        }

        private bool IsHeadless()
        {
#if UNITY_SERVER
            return true;
#endif
            return false;
        }
    }
}
