using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    [Serializable]
    public class PlaybackFile
    {
        public int Seed;
        public int InitialTick;
        public MatchSimulationStateS2C InitialSimulationState;
        public Dictionary<int, PlaybackTickData> Ticks;
    }
}