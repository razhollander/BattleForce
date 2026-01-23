using UnityEngine;

namespace Core.Scripts.Utils
{
    public static class PlaybackSettings
    {
        private const string PlaybackEnabledKey = "PlaybackEnabled";

        public static bool IsPlaybackEnabled
        {
            get => PlayerPrefs.GetInt(PlaybackEnabledKey, 0) == 1;
            set => PlayerPrefs.SetInt(PlaybackEnabledKey, value ? 1 : 0);
        }
    }
}
