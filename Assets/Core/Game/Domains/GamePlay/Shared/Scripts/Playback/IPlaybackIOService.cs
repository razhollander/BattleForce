using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    public interface IPlaybackIOService
    {
        List<string> GetAllPlaybackNames();
        bool TryGetPlayback(string playbackName, out PlaybackFile playbackFile);
        void SavePlayback(int _initialTick, int _seed, Dictionary<int, PlaybackTickData> _ticks, MatchSimulationStateS2C InitialSimulationState);
    }
}