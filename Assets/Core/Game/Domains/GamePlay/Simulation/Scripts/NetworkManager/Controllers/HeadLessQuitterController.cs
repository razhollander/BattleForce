using System;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Controllers
{
    public class HeadLessQuitterController : IInitializable, IDisposable, ITickable
    {
        private readonly IServerNetworkManager _serverNetworkManager;
        private readonly NetworkConfig _networkConfig;

        private float _lastPacketTime;

        public HeadLessQuitterController(IServerNetworkManager serverNetworkManager, NetworkConfig networkConfig)
        {
            _serverNetworkManager = serverNetworkManager;
            _networkConfig = networkConfig;
        }

        public void Initialize()
        {
            if (!IsHeadless()) return;

            _lastPacketTime = Time.time;
            _serverNetworkManager.PacketsListener.OnPacketReceivedEvent += OnPacketReceived;
            _serverNetworkManager.PacketsListener.OnPeerDisconnectedEvent += OnPeerDisconnected;
            LogService.LogTopic("HeadLessQuitterController Initialized", LogTopicType.ServerNetwork);
        }

        public void Dispose()
        {
            if (!IsHeadless()) return;

            _serverNetworkManager.PacketsListener.OnPacketReceivedEvent -= OnPacketReceived;
            _serverNetworkManager.PacketsListener.OnPeerDisconnectedEvent -= OnPeerDisconnected;
        }

        public void Tick()
        {
            if (!IsHeadless()) return;

            if (Time.time - _lastPacketTime > _networkConfig.HeadlessQuitTimeoutSeconds)
            {
                LogService.LogTopic($"[HeadLessQuitterController] No packets received for {_networkConfig.HeadlessQuitTimeoutSeconds} seconds. Quitting...", LogTopicType.ServerNetwork);
                Application.Quit();
            }
        }

        private void OnPacketReceived()
        {
            _lastPacketTime = Time.time;
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
        {
            // If all players are disconnected (count is 0), quit.
            if (peer.NetManager.ConnectedPeersCount == 0)
            {
                LogService.LogTopic("[HeadLessQuitterController] All players disconnected. Quitting...", LogTopicType.ServerNetwork);
                Application.Quit();
            }
        }

        private bool IsHeadless()
        {
            return Application.isBatchMode;
        }
    }
}
