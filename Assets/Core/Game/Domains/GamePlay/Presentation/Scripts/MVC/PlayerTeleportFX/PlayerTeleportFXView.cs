using System;
using System.Collections;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.PlayerTeleportFX
{
    public class PlayerTeleportFXView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float ShowDuration = 0.5f;
        [SerializeField] private ParticleSystem _particleSystem;

        public Action Despawn { get; set; }

        public void Play()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
            StartCoroutine(ReturnToPoolRoutine());
        }

        private IEnumerator ReturnToPoolRoutine()
        {
            yield return new WaitForSeconds(ShowDuration);
            Despawn?.Invoke();
        }

        public void OnCreated()
        {
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
