using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NetManagerPlayback : INetManagerWrapper
    {
        private readonly NetworkC2SPacketsListener _packetsListener;
        private readonly IPlaybackRecorderService _playbackRecorderService;
        private int _lastTick = 0;
        
        public NetManagerPlayback(NetworkC2SPacketsListener packetsListener, IPlaybackRecorderService playbackRecorderService)
        {
            _packetsListener = packetsListener;
            _playbackRecorderService = playbackRecorderService;
        }

        public bool IsRunning { get; private set; }
        public void Start(int port)
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void PollEvents()
        {
            var packets = _playbackRecorderService.GetPacketsForTick(_lastTick);
            _lastTick++;

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