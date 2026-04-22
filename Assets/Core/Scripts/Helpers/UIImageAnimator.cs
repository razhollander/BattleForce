using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts.Helpers
{
    public class UIImageAnimator : MonoBehaviour
    {
        [SerializeField] private bool _isLoop = true;
        [SerializeField] private List<Sprite> _frames;
        [SerializeField] private float _framesPerSecond = 10f;
        [SerializeField] private Image _uiImage;
        
        private CancellationTokenSource _animationCancellationTokenSource;

        public async Awaitable PlayAnimation(CancellationTokenSource cancellationTokenSource)
        {
            StopAnimation();
            _animationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            
            try 
            {
                await RunAnimationLoop(_animationCancellationTokenSource.Token);
            }
            catch (System.OperationCanceledException)
            {
                // Silently handle cancellation
            }
        }

        private async Awaitable RunAnimationLoop(CancellationToken token)
        {
            if (_frames == null || _frames.Count == 0) return;

            int currentFrame = 0;
            float delaySeconds = 1f / _framesPerSecond;

            while (!token.IsCancellationRequested)
            {
                _uiImage.sprite = _frames[currentFrame];
                
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
                        break;
                    }
                }
            }
            
            if (!_isLoop) StopAnimation();
        }

        public void StopAnimation()
        {
            if (_animationCancellationTokenSource == null) return;

            _animationCancellationTokenSource.Cancel();
            _animationCancellationTokenSource.Dispose();
            _animationCancellationTokenSource = null;
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
}