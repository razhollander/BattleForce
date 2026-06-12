using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    [Serializable]
    public class PlaybackFile
    {
        public int Seed;
        public int InitialTick;
        public string SimulationConfigJson;
        public EnterMatchPlayerData[] Players;
        public Dictionary<int, PlaybackTickData> Ticks;
    }
}