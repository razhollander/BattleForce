using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace CoreDomain.Scripts.Services.AudioService
{
    public abstract class AudioClipsScriptableObject : ScriptableObject
    {
        public SerializableDictionary<AudioClipType, AudioClip> AudioClips;
    }
}