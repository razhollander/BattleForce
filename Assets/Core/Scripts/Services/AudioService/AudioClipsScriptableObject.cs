using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Scripts.Services.AudioService
{
    public abstract class AudioClipsScriptableObject : ScriptableObject
    {
        public SerializableDictionary<AudioClipType, AudioClip> AudioClips;
    }
}