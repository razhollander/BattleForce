using System;
using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    [Serializable]
    public class PlaybackFile
    {
        public int Seed;
        public int InitialTick;
        public Dictionary<int, PlaybackTickData> Ticks;
    }
}