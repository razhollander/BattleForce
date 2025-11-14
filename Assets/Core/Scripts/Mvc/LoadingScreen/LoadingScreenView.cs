using System.Threading;
using CoreDomain.Scripts.Helpers;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.LoadingScreen
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private AnimatedSliderView _loadingSlider;
        [SerializeField] private Canvas _loadingScreenCanvas;

        public void ResetSlider()
        {
            _loadingSlider.ResetSlider();
        }
        
        public async Awaitable SetLoadingSlider(float valueBetween0To1, CancellationTokenSource cancellationTokenSource)
        {
            await _loadingSlider.AnimateSliderTo(valueBetween0To1, cancellationTokenSource);
        }

        public void Show()
        {
            _loadingScreenCanvas.enabled = true;
        }

        public void Hide()
        {
            _loadingScreenCanvas.enabled = false;
        }
    }
}
