using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Scripts.Services.AudioService
{
    public class AudioService : IAudioService
    {
        private const string POOLABLE_ONE_SHOT_AUDIO_OBJECT_NAME_FORMAT = "PoolableAudioSource-{0}";
        private const string POOLABLE_LOOP_AUDIO_OBJECT_NAME_FORMAT = "PoolableLoopAudioSource-{0}";
        
        private const int INITIAL_POOL_SIZE = 10;
        private const int POOL_INCREASE_STEP = 5;

        private readonly List<AudioClipsScriptableObject> _audioClipsScriptableObjects = new();
        private readonly List<AudioSourcePoolable> _activeOneShotAudioSources = new();
        private readonly Dictionary<AudioClipType, AudioSourcePoolable> _activeLoopAudioSources = new();
        private AudioSourcePool _audioSourcePool;

        [Inject]
        private void Construct(DiContainer diContainer, AudioSourcePoolable audioSourcePrefab)
        {
            _audioSourcePool = new AudioSourcePool(new PoolData(INITIAL_POOL_SIZE, POOL_INCREASE_STEP), diContainer, audioSourcePrefab);
        }

        public void InitEntryPoint()
        {
            _audioSourcePool.InitPool();
        }

        public void AddAudioClips(AudioClipsScriptableObject audioClipsScriptableObject)
        {
            _audioClipsScriptableObjects.Add(audioClipsScriptableObject);
        }

        public void RemoveAudioClips(AudioClipsScriptableObject audioClipsScriptableObject)
        {
            StopAllAudioClipsOfScriptableObject(audioClipsScriptableObject);
            _audioClipsScriptableObjects.Remove(audioClipsScriptableObject);
        }

        private void StopAllAudioClipsOfScriptableObject(AudioClipsScriptableObject audioClipsScriptableObject)
        {
            foreach (var kvp in audioClipsScriptableObject.AudioClips)
            {
                var audioClipId = kvp.Key;
                var audioClip = kvp.Value.Clip;

                if (_activeLoopAudioSources.ContainsKey(audioClipId))
                {
                    DespawnLoopSource(audioClipId);
                }

                for (var i = _activeOneShotAudioSources.Count - 1; i >= 0; i--)
                {
                    var poolable = _activeOneShotAudioSources[i];
                    if (poolable.AudioSource.clip != audioClip)
                    {
                        continue;
                    }

                    DespawnOneShotSource(poolable);
                }
            }
        }

        public void PlayRandomAudio(params AudioClipType[] audioClipTypes)
        {
            var randomIndex = Random.Range(0, audioClipTypes.Length);
            var audioClipType = audioClipTypes[randomIndex];
            PlayAudioAsync(audioClipType, Application.exitCancellationToken).Forget();
        }
        
        public void PlayAudio(AudioClipType audioClipType)
        {
            PlayAudioAsync(audioClipType, Application.exitCancellationToken).Forget();
        }

        
        public async Awaitable PlayAudioAsync(AudioClipType audioClipType, CancellationToken cancellationToken)
        {
            if (!TryGetAudioData(audioClipType, out var audioData))
            {
                return;
            }

            var poolable = _audioSourcePool.Spawn();
            poolable.name = POOLABLE_ONE_SHOT_AUDIO_OBJECT_NAME_FORMAT.Format(audioClipType);
            var source = poolable.AudioSource;
            source.clip = audioData.Clip;
            source.loop = false;
            source.SetAudioSourceVolume(audioData.Volume);
            source.Play();
            _activeOneShotAudioSources.Add(poolable);

            LogService.LogTopic($"Played Audio {audioClipType}", LogTopicType.Audio);

            try
            {
                await Awaitable.WaitForSecondsAsync(audioData.Clip.length, cancellationToken);
            }
            finally
            {
                DespawnOneShotSource(poolable);
            }
        }
        
        public void PlayAudioLoop(AudioClipType audioClipType)
        {
            if (!TryGetAudioData(audioClipType, out var audioData))
            {
                return;
            }

            var poolable = _audioSourcePool.Spawn();
            poolable.name = POOLABLE_LOOP_AUDIO_OBJECT_NAME_FORMAT.Format(audioClipType);
            var source = poolable.AudioSource;
            source.clip = audioData.Clip;
            source.loop = true;
            source.SetAudioSourceVolume(audioData.Volume);
            source.Play();

            StopLoopAudio(audioClipType);
            _activeLoopAudioSources[audioClipType] = poolable;

            LogService.LogTopic($"Played Audio {audioClipType}", LogTopicType.Audio);
        }

        public void StopLoopAudio(AudioClipType audioClipType)
        {
            DespawnLoopSource(audioClipType);
        }
        
        public void StopAllAudio()
        {
            LogService.LogTopic("Stop all audio", LogTopicType.Audio);
           
            for (var i = _activeOneShotAudioSources.Count - 1; i >= 0; i--)
            {
                DespawnOneShotSource(_activeOneShotAudioSources[i]);
            }

            var activeLoopAudioSources = _activeLoopAudioSources.Keys.ToList();
            for (var i = activeLoopAudioSources.Count - 1; i >= 0; i--)
            {
                DespawnLoopSource(activeLoopAudioSources[i]);
            }
        }

        private void DespawnOneShotSource(AudioSourcePoolable poolable)
        {
            if (poolable.Despawn == null)
            {
                return;
            }

            _activeOneShotAudioSources.Remove(poolable);
            var despawn = poolable.Despawn;
            poolable.Despawn = null;
            despawn.Invoke();
        }
        
        private void DespawnLoopSource(AudioClipType audioClipId)
        {
            if (!_activeLoopAudioSources.Remove(audioClipId, out var audioSource))
            {
                return;
            }

            var despawn = audioSource.Despawn;
            audioSource.Despawn = null;
            despawn.Invoke();
        }

        private bool TryGetAudioData(AudioClipType audioClipId, out AudioData audioData)
        {
            foreach (var audioClipsScriptableObject in _audioClipsScriptableObjects)
            {
                if (audioClipsScriptableObject.AudioClips.TryGetValue(audioClipId, out audioData))
                {
                    return true;
                }
            }

            LogService.LogError($"No clip of name {audioClipId} found");
            audioData = default;
            return false;
        }
    }
}
