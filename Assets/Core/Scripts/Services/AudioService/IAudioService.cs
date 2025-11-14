using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Services.AudioService
{
    public interface IAudioService
    {
        void InitEntryPoint();
        void PlayAudio(AudioClipType audioClipType, AudioChannelType audioChannel, AudioPlayType audioPlayType = AudioPlayType.OneShot);
        Awaitable PlayAudioAsync(AudioClipType audioClipType, AudioChannelType audioChannel, CancellationTokenSource cancellationTokenSource, AudioPlayType audioPlayType = AudioPlayType.OneShot);
        void StopAllAudio();
        void AddAudioClips(AudioClipsScriptableObject audioClipsScriptableObject);
        void RemoveAudioClips(AudioClipsScriptableObject audioClipsScriptableObject);
    }
}