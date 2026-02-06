using System;
using System.Linq;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    public class ChooseNetworkRoleUIController : IChooseNetworkRoleUIController
    {
        private readonly ChooseNetworkRoleUIView _uiView;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IClientNetworkManager _clientNetworkManager;
        private readonly NetworkConfig _networkConfig;

        public ChooseNetworkRoleUIController(ChooseNetworkRoleUIView uiView, ISceneLoaderService sceneLoaderService,
            IStateMachineService stateMachineService, IClientNetworkManager clientNetworkManager, NetworkConfig networkConfig)
        {
            _uiView = uiView;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _clientNetworkManager = clientNetworkManager;
            _networkConfig = networkConfig;
        }

        public void InitEntryPoint()
        {
            _uiView.Setup(OnClientClicked, OnHostClicked, OnServerClicked, OnPlayPlaybackClicked, _networkConfig.OnlyLocal, _networkConfig.IpAddress, _networkConfig.HostPort);
            PopulatePlaybacks();
#if UNITY_SERVER
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            StartServer(cancellationTokenSource).Forget();
#endif
        }

        private void PopulatePlaybacks()
        {
            var directory = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Records");
            if (System.IO.Directory.Exists(directory))
            {
                var files = System.IO.Directory.GetFiles(directory, "playback_*.json")
                    .OrderByDescending(f => System.IO.File.GetCreationTime(f))
                    .Select(System.IO.Path.GetFileName)
                    .ToList();
                _uiView.PlaybacksDropdown.ClearOptions();
                _uiView.PlaybacksDropdown.AddOptions(files);
            }
        }

        private void OnPlayPlaybackClicked()
        {
            _ = OnPlayPlaybackClickedAsync();
        }

        private async Awaitable OnPlayPlaybackClickedAsync()
        {
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            var selectedOptionIndex = _uiView.PlaybacksDropdown.value;
            if (selectedOptionIndex < 0 || selectedOptionIndex >= _uiView.PlaybacksDropdown.options.Count)
            {
                return;
            }
            var filename = _uiView.PlaybacksDropdown.options[selectedOptionIndex].text;

            try
            {
                await StartServer(cancellationTokenSource, true, filename);
                await StartClient(false, cancellationTokenSource);
                _uiView.Hide();
            }
            catch (Exception e)
            {
                LogService.LogException(e);
            }
        }

        private void OnServerClicked()
        {
            _ = OnServerClickedAsync();
        }

        private async Awaitable OnServerClickedAsync()
        {
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;

            try
            {
                await StartServer(cancellationTokenSource);
                _uiView.Hide();
            }
            catch (OperationCanceledException)
            {
                LogService.LogTopic("OperationCanceledException", LogTopicType.ClientNetwork);
            }
            catch (Exception e)
            {
                LogService.LogException(e);
            }
        }

        private void OnHostClicked()
        {
            _ = OnHostClickedAsync();
        }

        private async Awaitable OnHostClickedAsync()
        {
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;

            try
            {
                await StartServer(cancellationTokenSource);
                await StartClient(true, cancellationTokenSource);
                _uiView.Hide();
            }
            catch (OperationCanceledException)
            {
                LogService.LogTopic("OperationCanceledException", LogTopicType.ClientNetwork);
            }
            catch (Exception e)
            {
                LogService.LogException(e);
            }
        }

        private async Awaitable StartServer(CancellationTokenSource cancellationTokenSource, bool isPlayback = false, string playbackFile = "")
        {
            var enterData = new ServerInitiatorEnterData(isPlayback, playbackFile);
            LogService.LogTopic("Starting Server", LogTopicType.ClientNetwork);
            await _sceneLoaderService.TryLoadScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            LogService.LogTopic("Finished starting Server", LogTopicType.ClientNetwork);
        }
        
        private async Awaitable StartClient(bool isHost, CancellationTokenSource cancellationTokenSource)
        {
            LogService.LogTopic("Starting Client", LogTopicType.ClientNetwork);
            var ip = _uiView.IsLocalHost ? NetUtils.LOCAL_HOST_IP_ADDRESS : _uiView.IpAddress;
            var port = _uiView.Port;
            var playerName = _uiView.PlayerName;
            var enterData = new GamePlayMatchMakingInitiatorEnterData(ip, port, isHost);
            await LoadMatchMakingScene(enterData, cancellationTokenSource);
            _clientNetworkManager.StartClient(ip, port, playerName);
            LogService.LogTopic("Finished starting Client", LogTopicType.ClientNetwork);
        }

        private async Awaitable LoadMatchMakingScene(GamePlayMatchMakingInitiatorEnterData enterData, CancellationTokenSource cancellationTokenSource)
        {
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayMatchMakingScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayMatchMakingScene, enterData, cancellationTokenSource);
        }

        private void OnClientClicked()
        {
            _ = OnClientClickedAsync();
        }
        
        private async Awaitable OnClientClickedAsync()
        {
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;

            try
            {
                await StartClient(false, cancellationTokenSource);
                _uiView.Hide();
            }
            catch (OperationCanceledException)
            {
                LogService.LogTopic("OperationCanceledException", LogTopicType.ClientNetwork);
            }
            catch (Exception e)
            {
                LogService.LogException(e);
            }
        }
    }
}