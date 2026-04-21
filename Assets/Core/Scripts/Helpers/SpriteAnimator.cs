using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Core.Scripts.Helpers
{
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private bool _isLoop = true;
        [SerializeField] private List<Sprite> _frames;
        [SerializeField] private float _framesPerSecond = 10f;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private CancellationTokenSource _animationCancellationTokenSource;

        public async Awaitable PlayAnimation(CancellationTokenSource cancellationTokenSource)
        {
            // Clean up any existing animation task
            StopAnimation();
            _animationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            await RunAnimationLoop(_animationCancellationTokenSource.Token);
        }

        private async Awaitable RunAnimationLoop(CancellationToken token)
        {
            int currentFrame = 0;
            float delaySeconds = 1 / _framesPerSecond;

            while (true)
            {
                _spriteRenderer.sprite = _frames[currentFrame];
                await Awaitable.WaitForSecondsAsync(delaySeconds, cancellationToken: token);

                currentFrame++;

                if (currentFrame >= _frames.Count)
                {
                    if (_isLoop)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        StopAnimation();
                    }
                }
            }
        }

        public void StopAnimation()
        {
            var isAnimationRunning = _animationCancellationTokenSource != null;
            if (!isAnimationRunning)
            {
                return;
            }

            _animationCancellationTokenSource.Cancel();
            _animationCancellationTokenSource.Dispose();
            _animationCancellationTokenSource = null;
        }
    }
}