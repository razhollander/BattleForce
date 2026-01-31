using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    public class NetManagerPlayback : INetManagerWrapper
    {
        private NetworkC2SPacketsListener _packetsListener;
        private readonly IPlaybackRecorderService _playbackRecorderService;
        private readonly ITickService _tickService;

        public int ConnectedPeersCount => 1;

        public NetManagerPlayback(IPlaybackRecorderService playbackRecorderService, ITickService tickService)
        {
            _playbackRecorderService = playbackRecorderService;
            _tickService = tickService;
        }

        public void SetPacketsListener(NetworkC2SPacketsListener packetsListener)
        {
            _packetsListener = packetsListener;
        }

        //public bool IsRunning { get; private set; }

        public void Start(int port)
        {
            //IsRunning = true;
        }

        public void Stop()
        {
            //IsRunning = false;
        }

        public void PollEvents()
        {
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