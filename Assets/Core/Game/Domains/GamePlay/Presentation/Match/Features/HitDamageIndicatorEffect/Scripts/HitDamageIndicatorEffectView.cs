using System;
using System.Collections.Generic;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Helpers;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts
{
    public class HitDamageIndicatorEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private OutlinedTextView _text;
        
        [Header("Movement Settings")]
        [Tooltip("Minimum distance the text will travel to the left.")]
        [SerializeField] private float _minJumpDistanceX = 1.0f;
        [Tooltip("Maximum distance the text will travel to the left.")]
        [SerializeField] private float _maxJumpDistanceX = 2.0f;
        
        [Tooltip("Minimum height of the jump arc.")]
        [SerializeField] private float _minJumpPower = 0.8f;
        [Tooltip("Maximum height of the jump arc.")]
        [SerializeField] private float _maxJumpPower = 1.5f;
        
        [SerializeField] private float _verticalOffset = -0.5f;

        [Header("Scale Settings")]
        [Tooltip("The final size the text will reach.")]
        [SerializeField] private float _targetScale = 1.0f;
        [Tooltip("How long it takes to reach the target scale.")]
        [SerializeField] private float _scaleInDuration = 0.2f;
        
        [Header("Timing Settings")]
        [SerializeField] private float _showDurationInSeconds = 0.8f;
        [SerializeField] private float _textFadeInDuration = 0.1f;
        [SerializeField] private float _textFadeOutDuration = 0.2f;

        private readonly List<Awaitable> _animationTasks = new List<Awaitable>(2);
        
#if UNITY_EDITOR
        private CancellationTokenSource _editorTestCts;
#endif

        public async Awaitable PlayAndDespawn(ushort damage, Vector2 position, CancellationTokenSource cancellationTokenSource)
        {
            transform.position = position;
            _text.SetText($"-{damage}");
            
            var textAlpha = 0f;
            _text.SetAlpha(textAlpha);
            transform.localScale = Vector3.zero; 
            
            _animationTasks.Clear();

            var randomizedJumpX = Random.Range(_minJumpDistanceX, _maxJumpDistanceX);
            var randomizedJumpPower = Random.Range(_minJumpPower, _maxJumpPower);

            var endPosition = transform.localPosition + new Vector3(-randomizedJumpX, _verticalOffset, 0);
            
            _animationTasks.Add(transform.DOLocalJump(endPosition, randomizedJumpPower, 1, _showDurationInSeconds)
                .SetUpdate(true) 
                .SetEase(Ease.OutCubic)
                .WithCancellationSafe(cancellationTokenSource.Token));
            
            _animationTasks.Add(transform.DOScale(_targetScale, _scaleInDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WithCancellationSafe(cancellationTokenSource.Token));
            
            _animationTasks.Add(DOTween.To(()=>textAlpha, (x) =>
            {
                textAlpha = x;
                _text.SetAlpha(textAlpha);
            },1, _textFadeInDuration)
                .SetUpdate(true)
                .OnComplete(() =>
            {
                DOTween.To(()=>textAlpha, (x) =>
                    {
                        textAlpha = x;
                        _text.SetAlpha(textAlpha);
                    },0, _textFadeOutDuration)
                    .SetUpdate(true)
                    .SetDelay(_showDurationInSeconds - _textFadeInDuration - _textFadeOutDuration);
            }).WithCancellationSafe(cancellationTokenSource.Token));

            try
            {
                await _animationTasks.WhenAll();
            }
            finally
            {
                Despawn?.Invoke();
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