using System;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc
{
    public class ChickenEggView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Core.Scripts.Helpers.SpriteAnimator _breakAnimator;

        public void InterpolatePosition(Vector2 position, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(transform.position, position, decay, Time.deltaTime);
            SetPosition(lerpedPosition);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }


        }


        }


            gameObject.SetActive(false);
        }

        private System.Threading.CancellationTokenSource _breakCts;

        public void PlayBreakAnimation()
        {
            if (_breakAnimator != null)
            {
                _breakAnimator.gameObject.SetActive(true);
                if (_spriteRenderer != null) _spriteRenderer.enabled = false;
                _breakCts?.Cancel();
                _breakCts?.Dispose();
                _breakCts = new System.Threading.CancellationTokenSource();
                _breakAnimator.PlayAnimation(_breakCts).Forget();
            }
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            if (_spriteRenderer != null) _spriteRenderer.enabled = true;
            if (_breakAnimator != null)
            {
                _breakAnimator.StopAnimation();
                _breakAnimator.gameObject.SetActive(false);
            }
        }

        public void OnDespawned()
        {
            if (_breakAnimator != null)
            {
                _breakAnimator.StopAnimation();
            }
            _breakCts?.Cancel();
            _breakCts?.Dispose();
            _breakCts = null;
            gameObject.SetActive(false);
        }

    }
}
