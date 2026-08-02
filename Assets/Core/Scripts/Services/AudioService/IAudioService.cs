using System.Threading;
using UnityEngine;

namespace Core.Scripts.Services.AudioService
{
    public interface IAudioService
    {
        void InitEntryPoint();
        void PlayRandomAudio(params AudioClipType[] audioClipTypes);
        void PlayAudio(AudioClipType audioClipType);
        void PlayAudioLoop(AudioClipType audioClipType);
        void StopLoopAudio(AudioClipType audioClipType);
        int PlayAudioLoopWithId(AudioClipType audioClipType);
        int PlayAudioLoopWithId(AudioClip clip, float volume);
        void StopLoopAudioById(int loopId);
        Awaitable PlayAudioAsync(AudioClipType audioClipType, CancellationToken cancellationToken);
        void StopAllAudio();
        void AddAudioClips(AudioClipsScriptableObject audioClipsScriptableObject);
        void RemoveAudioClips(AudioClipsScriptableObject audioClipsScriptableObject);
    }
}
