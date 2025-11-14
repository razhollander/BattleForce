using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.Mvc.LoadingScreen
{
    public class LoadingScreenController : ILoadingScreenController
    {
        private readonly LoadingScreenView _loadingScreenView;
        
        [Inject]
        public LoadingScreenController(LoadingScreenView loadingScreenView)
        {
            _loadingScreenView = loadingScreenView;
        }

        public void Show()
        {
            LogService.LogTopic("Show loading screen", LogTopicType.LoadingScreen );
            _loadingScreenView.ResetSlider();
            _loadingScreenView.Show();
        }

        public void Hide()
        {
            LogService.LogTopic("Hide loading screen", LogTopicType.LoadingScreen );
            _loadingScreenView.Hide();
        }
        
        public void ResetSlider()
        {
            _loadingScreenView.ResetSlider();
        }
        
        public async Awaitable SetLoadingSlider(float valueBetween0To1, CancellationTokenSource cancellationTokenSource)
        {
            await _loadingScreenView.SetLoadingSlider(valueBetween0To1, cancellationTokenSource);
        }
    }
}