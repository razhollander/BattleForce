using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    [System.Serializable]
    public class PlaybackTickData
    {
        public int Tick;
        public List<RecordedPacket> Packets = new List<RecordedPacket>();
    }
}