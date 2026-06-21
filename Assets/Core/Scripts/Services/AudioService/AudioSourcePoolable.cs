using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Scripts.Services.AudioService
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourcePoolable : MonoBehaviour, IPoolable
    {
        [SerializeField] private AudioSource _audioSource;

        public AudioSource AudioSource => _audioSource;

        public Action Despawn { get; set; }

        public void OnCreated()
        {
            _audioSource.playOnAwake = false;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            _audioSource.Stop();
            _audioSource.clip = null;
            gameObject.SetActive(false);
        }
    }
}