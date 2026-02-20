using System;
using System.Threading;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GainedBoltEffect.Scripts
{
    public class GainBoltEffectView : MonoBehaviour, IPoolable
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _moveDistance = 1.0f;
        [SerializeField] private float _showDuration = 1.0f;
        [SerializeField] private float _textFaceInDuration = 0.2f;
        [SerializeField] private float _textFaceOutDuration = 0.2f;

        public async Awaitable PlayAndDespawn(int boltsAmount, Vector2 position, Transform parent, CancellationTokenSource cancellationTokenSource)
        {
            transform.position = position;
            transform.SetParent(parent);
            _text.text = $"+{boltsAmount}";

            var color = _text.color;
            color.a = 0;
            _text.color = color;
            var endYValue = transform.localPosition.y + _moveDistance;
            transform.DOLocalMoveY(endYValue, _showDuration).SetEase(Ease.OutQuad);
            _text.DOFade(1, _textFaceInDuration).OnComplete(() => { _text.DOFade(0, _textFaceOutDuration).SetDelay(_showDuration - _textFaceInDuration-_textFaceOutDuration).OnComplete(() => { gameObject.SetActive(false); }); });
            await Awaitable.WaitForSecondsAsync(_showDuration, cancellationToken: cancellationTokenSource.Token);
            Despawn();
        }

        public void OnCreated()
        {
            
        }

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
