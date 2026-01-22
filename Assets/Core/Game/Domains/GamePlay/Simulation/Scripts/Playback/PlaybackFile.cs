using System;
using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    [Serializable]
    public class PlaybackFile
    {
        public int Seed;
        public Dictionary<int, PlaybackTickData> Ticks;
    }
}