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
        
        private CancellationTokenSource _animationCts;

        public async Awaitable PlayAnimation(CancellationTokenSource cancellationTokenSource)
        {
            StopAnimation();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            
            try 
            {
                await RunAnimationLoop(_animationCts.Token);
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
            if (_animationCts == null) return;

            _animationCts.Cancel();
            _animationCts.Dispose();
            _animationCts = null;
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
}