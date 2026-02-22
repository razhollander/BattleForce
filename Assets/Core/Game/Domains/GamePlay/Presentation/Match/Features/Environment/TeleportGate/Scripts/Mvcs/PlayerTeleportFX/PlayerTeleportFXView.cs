using System;
using System.Collections;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportFX
{
    public class PlayerTeleportFXView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float ShowDuration = 0.5f;

        public Action Despawn { get; set; }

        public void Play()
        {
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
