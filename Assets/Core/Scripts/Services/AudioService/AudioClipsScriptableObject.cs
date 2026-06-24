using System;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Scripts.Services.AudioService
{
    [Serializable]
    public struct AudioData
    {
        public AudioClip Clip;
        [Tooltip("-1 = silent, 0 = half volume, 1 = full volume.")]
        [Range(-1f, 1f)]
        public float Volume;
    }

    public abstract class AudioClipsScriptableObject : ScriptableObject
    {
        public SerializableDictionary<AudioClipType, AudioData> AudioClips;
    }
}