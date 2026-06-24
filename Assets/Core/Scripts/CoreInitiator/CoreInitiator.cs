using System;
using System.Threading;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Network;
using Core.Scripts.Services.AudioService;
using Core.Scripts.Services.UnityThreadDispatcher;
using CoreDomain.Scripts.Audio;
using CoreDomain.Scripts.Mvc.LoadingScreen;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.SceneService;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.CoreInitiator
{
    public class CoreInitiator : MonoBehaviour
    {
        private GameInputActions _gameInputActions;
        private ISceneLoaderService _sceneLoaderService;
        private IAudioService _audioService;
        private ILoadingScreenController _loadingScreenController;
        private CoreAudioClipsScriptableObject _coreAudioClipsScriptableObject;
        private NetworkConfig _networkConfig;
        private IUnityMainThreadDispatcher _unityMainThreadDispatcher;
        private IWorldCameraController _worldCameraController;

        [Inject]
        private void Setup(GameInputActions gameInputActions, ISceneLoaderService sceneLoaderService, IAudioService audioService, ILoadingScreenController loadingScreenController,
            CoreAudioClipsScriptableObject coreAudioClipsScriptableObject, NetworkConfig networkConfig, IUnityMainThreadDispatcher unityMainThreadDispatcher, IWorldCameraController worldCameraController)
        {
            _gameInputActions = gameInputActions;
            _sceneLoaderService = sceneLoaderService;
            _audioService = audioService;
            _loadingScreenController = loadingScreenController;
            _coreAudioClipsScriptableObject = coreAudioClipsScriptableObject;
            _networkConfig = networkConfig;
            _unityMainThreadDispatcher = unityMainThreadDispatcher;
            _worldCameraController = worldCameraController;
        }

        private void Start()
        {
            _ = InitEntryPoint(CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken));
        }

        private async Awaitable InitEntryPoint(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                UpdateApplicationSettings();
                // _loadingScreenController.Show();
                InitializeServices();
                InitializeControllers();
                _audioService.AddAudioClips(_coreAudioClipsScriptableObject);
                await LoadGameScene(cancellationTokenSource);
                //await _loadingScreenController.SetLoadingSlider(1, cancellationTokenSource);
                int i = 0;
            }
            catch (OperationCanceledException)
            {
                LogService.Log("Operation init core was cancelled");
            }
            catch (Exception e)
            {
                LogService.LogError(e.Message);
                throw;
            }
            
            //_loadingScreenController.Hide();
        }
        
        private void UpdateApplicationSettings()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = _networkConfig.TicksPerSeconds;
            Application.runInBackground = true;
            Time.fixedDeltaTime = 1f / _networkConfig.TicksPerSeconds;
        }

        private void InitializeServices()
        {
            _gameInputActions.Enable();
            _audioService.InitEntryPoint();
            _sceneLoaderService.InitEntryPoint();
            _unityMainThreadDispatcher.InitEntryPoint();
        }

        private void InitializeControllers()
        {
            _worldCameraController.InitEntryPoint();
        }

        private void OnDestroy()
        {
            _worldCameraController.InitExitPoint();
        }

        private async Awaitable LoadGameScene(CancellationTokenSource cancellationTokenSource)
        {
            await _sceneLoaderService.TryLoadScene(SceneType.GameScene, new GameInitiatorEnterData(), cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GameScene, new GameInitiatorEnterData(), cancellationTokenSource);
        }
    }
}