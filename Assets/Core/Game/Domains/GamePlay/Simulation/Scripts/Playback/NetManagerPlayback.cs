using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    public class NetManagerPlayback : INetManagerWrapper
    {
        private NetworkC2SPacketsListener _packetsListener;
        private readonly IPlaybackRecorderService _playbackRecorderService;
        private readonly ITickService _tickService;
        private readonly NetworkConfig _networkConfig;
        private NetManager _netManager;

        public int ConnectedPeersCount => _netManager != null ? _netManager.ConnectedPeersCount : 0;

        public NetManagerPlayback(IPlaybackRecorderService playbackRecorderService, ITickService tickService, NetworkConfig networkConfig)
        {
            _playbackRecorderService = playbackRecorderService;
            _tickService = tickService;
            _networkConfig = networkConfig;
        }

        public void SetPacketsListener(NetworkC2SPacketsListener packetsListener)
        {
            _packetsListener = packetsListener;
            _netManager = new NetManager(_packetsListener/*new EmptyNetworkC2SPacketsListener(_networkConfig)*/) { AutoRecycle = true, BroadcastReceiveEnabled = true, IPv6Enabled = false };
        }

        public void Start(int port)
        {
            _netManager?.Start(port);
        }

        public void Stop()
        {
            _netManager?.Stop();
        }

        public void PollEvents()
        {
            LogService.LogError("Poll from playback!");
            _netManager?.PollEvents();

            var packets = _playbackRecorderService.GetPacketsForTick(_tickService.CurrentTick);

            if (packets.IsNullOrEmpty())
            {
                return;
            }

            foreach (var packet in packets)
            {
                var dummyPeer = (NetPeer) System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(NetPeer));

                if (packet.PlayerId != ushort.MaxValue) // todo dont send a peer but instead send player id
                {
                    dummyPeer.Tag = packet.PlayerId;
                }
                    
                var reader = new NetDataReader(packet.Data);
                _packetsListener.OnNetworkReceive(dummyPeer, reader);
            }
        }
    }
}
