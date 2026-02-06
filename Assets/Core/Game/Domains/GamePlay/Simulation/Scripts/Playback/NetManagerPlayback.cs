using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    public class NetManagerPlayback : INetManagerWrapper
    {
        private NetworkC2SPacketsListener _packetsListener;
        private readonly IPlaybackRecorderService _playbackRecorderService;
        private readonly ITickService _tickService;
        private NetManager _netManager;

        public int ConnectedPeersCount => _netManager != null ? _netManager.ConnectedPeersCount : 0;

        public NetManagerPlayback(IPlaybackRecorderService playbackRecorderService, ITickService tickService)
        {
            _playbackRecorderService = playbackRecorderService;
            _tickService = tickService;
        }

        public void SetPacketsListener(NetworkC2SPacketsListener packetsListener)
        {
            _packetsListener = packetsListener;
            _netManager = new NetManager(packetsListener) { AutoRecycle = true, BroadcastReceiveEnabled = true, IPv6Enabled = false };
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
            _netManager?.PollEvents();

            var packets = _playbackRecorderService.GetPacketsForTick(_tickService.CurrentTick);

            if (packets.IsNullOrEmpty())
            {
                return;
            }

            foreach (var p in packets)
            {
                var dummyPeer = (NetPeer) System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(NetPeer));

                if (p.PlayerId != ushort.MaxValue) // todo dont send a peer but instead send player id
                {
                    dummyPeer.Tag = p.PlayerId;
                }
                    
                var reader = new NetDataReader(p.Data);
                _packetsListener.OnNetworkReceive(dummyPeer, reader);
            }
        }
    }
}
