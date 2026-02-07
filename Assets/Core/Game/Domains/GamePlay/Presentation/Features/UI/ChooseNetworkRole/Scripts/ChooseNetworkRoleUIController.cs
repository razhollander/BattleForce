using System;
using System.Linq;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller;
using Core.Scripts.Network;
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
        private readonly IPlaybackIOService _playbackIOService;

        public ChooseNetworkRoleUIController(ChooseNetworkRoleUIView uiView, ISceneLoaderService sceneLoaderService,
            IStateMachineService stateMachineService, IClientNetworkManager clientNetworkManager, NetworkConfig networkConfig, IPlaybackIOService playbackIOService)
        {
            _uiView = uiView;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _clientNetworkManager = clientNetworkManager;
            _networkConfig = networkConfig;
            _playbackIOService = playbackIOService;
        }

        public void InitEntryPoint()
        {
            _uiView.Setup(OnClientClicked, OnHostClicked, OnServerClicked, OnPlayPlaybackClicked, _networkConfig.OnlyLocal, _networkConfig.IpAddress, _networkConfig.HostPort);
            PopulatePlaybacksDropdown();
#if UNITY_SERVER
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            StartServer(cancellationTokenSource, false).Forget();
#endif
        }

        private void PopulatePlaybacksDropdown()
        {
            _uiView.PlaybacksDropdown.ClearOptions();
            _uiView.PlaybacksDropdown.AddOptions(_playbackIOService.GetAllPlaybackNames());
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
                await StartClient(true, cancellationTokenSource, true, filename);
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
                await StartServer(cancellationTokenSource, false);
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
                await StartServer(cancellationTokenSource, false);
                await StartClient(true, cancellationTokenSource, false);
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

        private async Awaitable StartServer(CancellationTokenSource cancellationTokenSource, bool isPlaybackEnabled, string playbackFilePath = "")
        {
            var enterData = new ServerInitiatorEnterData(isPlaybackEnabled, playbackFilePath);
            LogService.LogTopic("Starting Server", LogTopicType.ClientNetwork);
            await _sceneLoaderService.TryLoadScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            LogService.LogTopic("Finished starting Server", LogTopicType.ClientNetwork);
        }
        
        private async Awaitable StartClient(bool isHost, CancellationTokenSource cancellationTokenSource, bool isPlaybackEnabled, string playbackFilePath = "")
        {
            LogService.LogTopic("Starting Client", LogTopicType.ClientNetwork);
            var ip = _uiView.IsLocalHost ? NetUtils.LOCAL_HOST_IP_ADDRESS : _uiView.IpAddress;
            var port = _uiView.Port;
            var playerName = _uiView.PlayerName;

            if (isPlaybackEnabled && _playbackIOService.TryGetPlayback(playbackFilePath, out var playbackFile))
            {
                var InitialState = playbackFile.InitialSimulationState;
                var enterData = new GamePlayMatchInitiatorEnterData(InitialState, InitialState.Players[0].Id);
                await LoadMatchScene(enterData, cancellationTokenSource);
            }
            else
            {
                var enterData = new GamePlayMatchMakingInitiatorEnterData(ip, port, isHost);
                await LoadMatchMakingScene(enterData, cancellationTokenSource);
            }
            
            _clientNetworkManager.StartClient(ip, port, playerName);
            LogService.LogTopic("Finished starting Client", LogTopicType.ClientNetwork);
        }

        private async Awaitable LoadMatchMakingScene(GamePlayMatchMakingInitiatorEnterData enterData, CancellationTokenSource cancellationTokenSource)
        {
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayMatchMakingScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayMatchMakingScene, enterData, cancellationTokenSource);
        }  
        
        private async Awaitable LoadMatchScene(GamePlayMatchInitiatorEnterData enterData, CancellationTokenSource cancellationTokenSource)
        {
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayMatchScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayMatchScene, enterData, cancellationTokenSource);
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
                await StartClient(false, cancellationTokenSource, false);
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