using System;
using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    [Serializable]
    public class PlaybackDebugData
    {
        public int Seed;
        public int InitialTick;
        public List<DebugTickData> Ticks = new List<DebugTickData>();
    }
}