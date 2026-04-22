using System;
using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerChickenView : MonoBehaviour
    {
        [SerializeField] private GameObject _chickenBodyGameObject;
        [SerializeField] private GameObject _chickenCrestGameObject;
        [SerializeField] private Sprite _chickenLaySprite;
        [SerializeField] private Sprite _chickenIdleSprite;
        [SerializeField] private SpriteRenderer _chikenBodySpriteRenderer;
        [SerializeField] private float _layAnimationDurationInSeconds;
    
        private CancellationTokenSource _layEggCancellationTokenSource;

        public void SetChickenState(bool isOn)
        {
            _chickenBodyGameObject.SetActive(isOn);
            _chickenCrestGameObject.SetActive(isOn);
            if (isOn)
            {
                return;
            }

            StopLayEggAnimation();
        }

        public async Awaitable PlayLayEggAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _chikenBodySpriteRenderer.sprite = _chickenLaySprite;
            StopLayEggAnimation();
            _layEggCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);

            try
            {
                await Awaitable.WaitForSecondsAsync(_layAnimationDurationInSeconds, cancellationTokenSource.Token);
            }
            finally
            {
                _chikenBodySpriteRenderer.sprite = _chickenIdleSprite;
            }
        }

        private void StopLayEggAnimation()
        {
            _layEggCancellationTokenSource?.Cancel();
            _layEggCancellationTokenSource?.Dispose();
            _layEggCancellationTokenSource = null;
        }
    }
}
