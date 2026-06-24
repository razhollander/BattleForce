using System;
using System.Collections.Generic;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts
{
    public class HitDamageIndicatorEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _moveDistance = 0.8f;
        [SerializeField] private float _showDurationInSeconds = 0.8f;
        [SerializeField] private float _textFadeInDuration = 0.1f;
        [SerializeField] private float _textFadeOutDuration = 0.2f;

        private readonly List<Awaitable> _animationTasks = new List<Awaitable>(2);

        public async Awaitable PlayAndDespawn(ushort damage, Vector2 position, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            transform.position = position;
            transform.SetParent(parent);
            _text.text = $"-{damage}";

            var color = _text.color;
            color.a = 0;
            _text.color = color;
            var endYValue = transform.localPosition.y + _moveDistance;
            _animationTasks.Clear();
            _animationTasks.Add(transform.DOLocalMoveY(endYValue, _showDurationInSeconds).SetEase(Ease.OutQuad).WithCancellationSafe(cancellationTokenSource.Token));
            _animationTasks.Add(_text.DOFade(1, _textFadeInDuration).OnComplete(() =>
            {
                _text.DOFade(0, _textFadeOutDuration).SetDelay(_showDurationInSeconds - _textFadeInDuration - _textFadeOutDuration);
            }).WithCancellationSafe(cancellationTokenSource.Token));

            try
            {
                await _animationTasks.WhenAll();
            }
            finally
            {
                Despawn();
            }
        }

        public void OnCreated() { }

        public Action Despawn { get; set; }

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
