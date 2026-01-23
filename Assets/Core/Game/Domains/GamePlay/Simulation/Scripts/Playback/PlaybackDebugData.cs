using System;
using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    [Serializable]
    public class PlaybackDebugData
    {
        public int Seed;
        public List<DebugTickData> Ticks = new List<DebugTickData>();
    }
}