using System;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NetManagerWrapper : INetManagerWrapper, IDisposable
    {
        private NetManager _netManager;

        //public bool IsRunning => _netManager.IsRunning;
        public int ConnectedPeersCount => _netManager.ConnectedPeersCount;

        public NetManagerWrapper()
        {
        }

        public void SetPacketsListener(NetworkC2SPacketsListener packetsListener)
        {
            _netManager = new NetManager(packetsListener) { AutoRecycle = true, BroadcastReceiveEnabled = true, IPv6Enabled = false};
        }

        public void Start(int port)
        {
            _netManager.Start(port);
        }

        public void Stop()
        {
            _netManager.Stop();
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }
        
        public void Dispose()
        {
            LogService.LogError("NetManagerWrapper disposed");
        }
    }
}