using UnityEngine;
using System.Collections;
using Core.Scripts.Utils.Pools;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.PlayerTeleportFX
{
    public class PlayerTeleportFXView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float ShowDuration = 0.5f;
        [SerializeField] private ParticleSystem _particleSystem;

        private System.Action<PlayerTeleportFXView> _returnToPool;

        public void Init(System.Action<PlayerTeleportFXView> returnToPool)
        {
            _returnToPool = returnToPool;
        }

        public void Play()
        {
            gameObject.SetActive(true);
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
            StartCoroutine(ReturnToPoolRoutine());
        }

        private IEnumerator ReturnToPoolRoutine()
        {
            yield return new WaitForSeconds(ShowDuration);
            _returnToPool?.Invoke(this);
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
        }

        public void OnGetFromPool()
        {
            // Reset state if needed
        }

        public class Pool : MonoMemoryPool<PlayerTeleportFXView>
        {
        }
    }
}
