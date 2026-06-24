using UnityEngine;

namespace Core.Scripts.Utils
{
    public static class AudioUtils
    {
        public static void SetAudioSourceVolume(this AudioSource source, float volume)
        {
            source.volume = (volume + 1f) / 2f;
        }
    }
}