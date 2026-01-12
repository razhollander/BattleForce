using System.Collections;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;
using System;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardObtainedEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer _lineRenderer;

        private Action<TalentCardObtainedEffectView> _returnToPoolAction;

        public void Init(Action<TalentCardObtainedEffectView> returnToPoolAction)
        {
            _returnToPoolAction = returnToPoolAction;
        }

        public void Play(Vector2 from, Vector2 to, float duration)
        {
            _lineRenderer.SetPosition(0, from);
            _lineRenderer.SetPosition(1, to);
            StartCoroutine(ReturnToPoolAfterDelay(duration));
        }

        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _returnToPoolAction?.Invoke(this);
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
