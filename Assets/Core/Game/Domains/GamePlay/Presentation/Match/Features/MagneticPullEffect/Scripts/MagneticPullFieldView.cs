using System;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.Logger.Base;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts
{
    public class MagneticPullFieldView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _showDurationInSeconds = 1f;
        [SerializeField] private float _fadeDurationInSeconds = 0.2f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public Action Despawn { get; set; }

        public async Awaitable PlayAndDespawn(Vector2 position, Vector2 rotation, float size, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            transform.position = position;
            transform.rotation = rotation.ToQuaternion();
            transform.localScale = new Vector3(size, size, 1f);
            transform.SetParent(parent);
            
            var color = _spriteRenderer.color;
            color.a = 1;
            _spriteRenderer.color = color;

            try
            {
                await Awaitable.WaitForSecondsAsync(_showDurationInSeconds - _fadeDurationInSeconds, cancellationTokenSource.Token);
                await _spriteRenderer.DOFade(0, _fadeDurationInSeconds).WithCancellationSafe(cancellationTokenSource.Token);
            }
            finally
            {
                Despawn();
            }
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