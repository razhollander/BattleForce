using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    public interface IPlaybackIOService
    {
        List<string> GetAllPlaybackNames();
        bool TryGetPlayback(string playbackName, out PlaybackFile playbackFile);
        void SavePlayback(int initialTick, int seed, Dictionary<int, PlaybackTickData> ticks, EnterMatchPlayerData[] players);
    }
}