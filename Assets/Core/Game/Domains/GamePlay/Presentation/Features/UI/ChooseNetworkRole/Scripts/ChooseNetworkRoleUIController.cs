using System;
using System.Linq;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.DataPersistence;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    public class ChooseNetworkRoleUIController : IChooseNetworkRoleUIController
    {
        private const string PREFS_PLAYER_NAME_KEY = "NetworkRole_PlayerName";
        private const string PREFS_IP_ADDRESS_KEY = "NetworkRole_IpAddress";
        private const string PREFS_IS_LOCAL_HOST_KEY = "NetworkRole_IsLocalHost";
        private const string PREFS_PORT_HOST_KEY = "NetworkRole_Port";
        
        private readonly ChooseNetworkRoleUIView _uiView;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IStateMachineService _stateMachineService;
        private readonly NetworkConfig _networkConfig;
        private readonly IPlaybackIOService _playbackIOService;
        private readonly ICommandFactory _commandFactory;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly IDataPersistence _dataPersistence;

        public ChooseNetworkRoleUIController(ChooseNetworkRoleUIView uiView, ISceneLoaderService sceneLoaderService,
            IStateMachineService stateMachineService, NetworkConfig networkConfig, IPlaybackIOService playbackIOService,
            ICommandFactory commandFactory, SharedGamePlayConfig sharedGamePlayConfig, IDataPersistence dataPersistence)
        {
            _uiView = uiView;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _networkConfig = networkConfig;
            _playbackIOService = playbackIOService;
            _commandFactory = commandFactory;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _dataPersistence = dataPersistence;
        }

        public void InitEntryPoint()
        {
            var defaultPlayerName = "Player_" + UnityEngine.Random.Range(1000, 9999);
            var playerName = _dataPersistence.Load(PREFS_PLAYER_NAME_KEY, defaultPlayerName);
            var ipAddress = _dataPersistence.Load(PREFS_IP_ADDRESS_KEY, _networkConfig.IpAddress);
            var isLocalHost = _dataPersistence.Load(PREFS_IS_LOCAL_HOST_KEY, _networkConfig.OnlyLocal);
            var port = _dataPersistence.Load(PREFS_PORT_HOST_KEY, _networkConfig.DefaultHostPort);
            
            _uiView.Setup(OnClientClicked, OnHostClicked, OnServerClicked, OnPlayPlaybackClicked, isLocalHost, ipAddress, port, playerName);
            PopulatePlaybacksDropdown();

            if (PlayerPrefsSettings.ShouldSkipMatchMaking)
            {
                OnHostClicked();
            }
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
            var playbackName = _uiView.GetSelectedPlayback();

            try
            {
                await StartServer(cancellationTokenSource, true, playbackName);
                StartClient(NetUtils.LOCAL_HOST_IP_ADDRESS, true, cancellationTokenSource, true, playbackName);
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
            SaveLocallyChosenParameters();
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;

            try
            {
                await StartServer(cancellationTokenSource, false);
                StartClient(NetUtils.LOCAL_HOST_IP_ADDRESS, true, cancellationTokenSource, false);
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
            SaveLocallyChosenParameters();
            var port = _uiView.Port;
            var enterData = new ServerInitiatorEnterData(isPlaybackEnabled, playbackFilePath, port);
            LogService.LogTopic("Starting Server", LogTopicType.ClientNetwork);
            await _sceneLoaderService.TryLoadScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            LogService.LogTopic("Finished starting Server", LogTopicType.ClientNetwork);
        }

        private void StartClient(string ip, bool isHost, CancellationTokenSource cancellationTokenSource, bool isPlaybackEnabled, string playbackName = "")
        {
            LogService.LogTopic("Starting Client", LogTopicType.ClientNetwork);
            var port = _uiView.Port;
            var playerName = GetPlayerName(isPlaybackEnabled, playbackName);
            
            _commandFactory.CreateCommandAsync<StartClientCommand>()
                .SetIsHost(isHost)
                .SetServerAddress(ip,port)
                .SetPlayerName(playerName)
                .Execute(cancellationTokenSource).Forget();
            
            LogService.LogTopic("Finished starting Client", LogTopicType.ClientNetwork);
        }

        private string GetPlayerName(bool isPlaybackEnabled, string playbackName = "")
        {
            var playerName = _uiView.PlayerName;

            if (isPlaybackEnabled)
            {
                _playbackIOService.TryGetPlayback(playbackName, out var playbackFile);
                playerName = playbackFile.Players[0].Name;
            }
            else if (PlayerPrefsSettings.ShouldSkipMatchMaking)
            {
                playerName = _sharedGamePlayConfig.DefaultMatchEnterDataConfig.DefaultSimulationMatchEnterData.Players[0].Name;
            }

            return playerName;
        }

        private void SaveLocallyChosenParameters()
        {
            _dataPersistence.Save(
                PREFS_PLAYER_NAME_KEY, _uiView.PlayerName,
                PREFS_IP_ADDRESS_KEY, _uiView.IpAddress, 
                PREFS_IS_LOCAL_HOST_KEY, _uiView.IsLocalHost,
                PREFS_PORT_HOST_KEY, _uiView.Port);
        }

        private void OnClientClicked()
        {
            SaveLocallyChosenParameters();
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            var ip = _uiView.IsLocalHost ? NetUtils.LOCAL_HOST_IP_ADDRESS : _uiView.IpAddress;
            StartClient(ip, false, cancellationTokenSource, false);
            _uiView.Hide();
        }
    }
}