using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    [Serializable]
    public class PlaybackFile
    {
        public int Seed;
        public int InitialTick;
        public Dictionary<int, PlaybackTickData> Ticks;
        public SimulationMatchEnterData.PlayerData[] Players;
    }
}