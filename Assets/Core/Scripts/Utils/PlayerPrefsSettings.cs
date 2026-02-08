using UnityEngine;

namespace Core.Scripts.Utils
{
    public static class PlayerPrefsSettings
    {
        public const string SkipMatchMakingKey = "SkipMatchMaking";
        
        public static bool ShouldSkipMatchMaking
        {
            get => PlayerPrefs.GetInt(SkipMatchMakingKey, 0) == 1;
            set => PlayerPrefs.SetInt(SkipMatchMakingKey, value ? 1 : 0);
        }
    }
}
