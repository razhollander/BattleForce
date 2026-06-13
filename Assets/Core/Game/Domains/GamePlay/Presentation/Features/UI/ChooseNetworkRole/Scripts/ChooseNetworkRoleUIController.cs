using System;
using System.Collections.Generic;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.DataPersistence;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    public class ChooseNetworkRoleUIController : IChooseNetworkRoleUIController
    {
        private const string PREFS_PLAYERS_JOINED_KEY = "NetworkRole_PlayersJoined";
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
        private readonly IInputDeviceChangedListenerService _nativeInputDeviceService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;

        private List<PlayerJoinedModel> _playerJoinedModels = new List<PlayerJoinedModel>();
        
        public ChooseNetworkRoleUIController(ChooseNetworkRoleUIView uiView, ISceneLoaderService sceneLoaderService,
            IStateMachineService stateMachineService, NetworkConfig networkConfig, IPlaybackIOService playbackIOService,
            ICommandFactory commandFactory, SharedGamePlayConfig sharedGamePlayConfig, IDataPersistence dataPersistence, IInputDeviceChangedListenerService nativeInputDeviceService, PresentationGamePlayConfig gamePlayConfig)
        {
            _uiView = uiView;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _networkConfig = networkConfig;
            _playbackIOService = playbackIOService;
            _commandFactory = commandFactory;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _dataPersistence = dataPersistence;
            _nativeInputDeviceService = nativeInputDeviceService;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            var currentPlayersJoinedModels = GetAllPlayerJoinedModels();
            _playerJoinedModels = currentPlayersJoinedModels;
            var ipAddress = _dataPersistence.Load(PREFS_IP_ADDRESS_KEY, _networkConfig.IpAddress);
            var isLocalHost = _dataPersistence.Load(PREFS_IS_LOCAL_HOST_KEY, _networkConfig.OnlyLocal);
            var port = _dataPersistence.Load(PREFS_PORT_HOST_KEY, _networkConfig.DefaultHostPort);
            
            _uiView.Setup(OnClientClicked, OnHostClicked, OnServerClicked, OnPlayPlaybackClicked, OnPlayerNameChanged, OnRemovePlayerButtonClicked,isLocalHost, ipAddress, port, currentPlayersJoinedModels);
            PopulatePlaybacksDropdown();
            if (PlayerPrefsSettings.ShouldSkipMatchMaking)
            {
                StartHost().Forget();
            }
            else
            {
                _nativeInputDeviceService.GamepadAddedEvent += OnGamepadAdded;
                _nativeInputDeviceService.GamepadRemovedEvent += OnGamepadRemoved;
            }
#if UNITY_SERVER
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            StartServer(cancellationTokenSource, false).Forget();
#endif
        }

        private void OnGamepadRemoved(Gamepad gamepad)
        {
            var playerJoinedIndex = _playerJoinedModels.FindIndex(p => p.InputDeviceId == gamepad.deviceId);
            var didFindPlayer = playerJoinedIndex != -1;

            if (!didFindPlayer)
            {
                LogService.LogError("Didn't find player for device id " + gamepad.deviceId + " when removing gamepad.");
                return;
            }
            LogService.LogError($"Remove index {playerJoinedIndex} for device id {gamepad.deviceId}");
            _uiView.RemovePlayerJoined(playerJoinedIndex);
            _playerJoinedModels.RemoveAt(playerJoinedIndex);
        }

        public void InitExitPoint()
        {
            _nativeInputDeviceService.GamepadAddedEvent -= OnGamepadAdded;
            _nativeInputDeviceService.GamepadRemovedEvent -= OnGamepadRemoved;
        }

        private List<PlayerJoinedModel> GetAllPlayerJoinedModels()
        {
            var playerJoinedModels = new List<PlayerJoinedModel>();
            var currentlyConnectedKeyboards = _nativeInputDeviceService.GetAllConnectedKeyboards();
            for (var i = 0; i < currentlyConnectedKeyboards.Count; i++)
            {
                playerJoinedModels.Add(GetSavedPlayerJoinedModelByDeviceOrDefault(currentlyConnectedKeyboards[i].deviceId, SupportedInputType.Mouse));
            }
            
            var currentlyConnectedGamepads = _nativeInputDeviceService.GetAllConnectedGamepads();
            for (var i = 0; i < currentlyConnectedGamepads.Count; i++)
            {
                playerJoinedModels.Add(GetSavedPlayerJoinedModelByDeviceOrDefault(currentlyConnectedGamepads[i].deviceId, SupportedInputType.Gamepad));
            }
            return playerJoinedModels;
        }
        
        private PlayerJoinedModel GetSavedPlayerJoinedModelByDeviceOrDefault(int deviceId, SupportedInputType playerInputType)
        {
            var defaultPlayerName = "Player_" + UnityEngine.Random.Range(10000, 99999);
            var defaultPlayerJoined = new PlayerJoinedModel(defaultPlayerName, playerInputType, deviceId); 
            var defaultPlayerJoinedModels = new List<PlayerJoinedModel> {defaultPlayerJoined};
            var playerJoinedModels = _dataPersistence.Load(PREFS_PLAYERS_JOINED_KEY, defaultPlayerJoinedModels);
            var playerJoinedWithDevice = playerJoinedModels.Find(x => x.InputDeviceId == deviceId);

            if (playerJoinedWithDevice != null)
            {
                return playerJoinedWithDevice;
            }

            return defaultPlayerJoined;
        }
        
        private void OnGamepadAdded(Gamepad gamepad)
        {
            var isDeviceAlreadyJoinedDueToReconnect = _playerJoinedModels.Exists(p => p.InputDeviceId == gamepad.deviceId);
            if (isDeviceAlreadyJoinedDueToReconnect)
            {
                return; 
            }
            
            var playerJoinedModel = GetSavedPlayerJoinedModelByDeviceOrDefault(gamepad.deviceId,SupportedInputType.Gamepad);
            _playerJoinedModels.Add(playerJoinedModel);
            _uiView.AddPlayerJoinedPanel(playerJoinedModel.PlayerName, playerJoinedModel.PlayerInputType);
        }

        private void OnRemovePlayerButtonClicked(int playerIndex)
        {
            _playerJoinedModels.RemoveAt(playerIndex);
            _uiView.RemovePlayerJoined(playerIndex);
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
            StartHost().Forget();
        }

        private async Awaitable StartHost()
        {
            SaveLocallyChosenParameters();
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            await StartServer(cancellationTokenSource, false);
            StartClient(NetUtils.LOCAL_HOST_IP_ADDRESS, true, cancellationTokenSource, false);
            _uiView.Hide();
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
            var playersJoinedModels = GetPlayersJoined(isPlaybackEnabled, playbackName);
            var clientId = _gamePlayConfig.ShouldOverrideClientId ? _gamePlayConfig.ClientIdOverride : NetworkUtils.GetDeviceUniqueId();
            
            _commandFactory.CreateCommandAsync<StartClientCommand>()
                .SetIsHost(isHost)
                .SetServerAddress(ip,port)
                .SetClientId(clientId)
                .SetPlayersJoined(playersJoinedModels)
                .Execute(cancellationTokenSource).Forget();
            
            LogService.LogTopic("Finished starting Client", LogTopicType.ClientNetwork);
        }
        
        private List<PlayerJoinedModel> GetPlayersJoined(bool isPlaybackEnabled, string playbackName)
        {
            if (isPlaybackEnabled)
            {
                _playbackIOService.TryGetPlayback(playbackName, out var playbackFile);
                var players = playbackFile.Players;
                var playersJoined = ConvertPlayersEnteredToPlayerJoined(players);
                return playersJoined;
            }

            if (PlayerPrefsSettings.ShouldSkipMatchMaking)
            {
                var  players = _sharedGamePlayConfig.DefaultMatchEnterDataConfig.Players;
                var playersJoined = ConvertPlayersEnteredToPlayerJoined(players);
                return playersJoined;
            }

            return _playerJoinedModels;
        }

        private List<PlayerJoinedModel> ConvertPlayersEnteredToPlayerJoined(EnterMatchPlayerData[] players)
        {
            var playbackPlayersJoinedModels = new List<PlayerJoinedModel>();
            var localPlayerConnectedDeviceId = GetFirstLocalPlayerConnectedDeviceId(out var localPlayerDeviceType);

            for (int i = 0; i < players.Length; i++)
            {
                var isFirstPlayer = i == 0;
                var deviceId = isFirstPlayer ? localPlayerConnectedDeviceId : -i; // its -i because to not conflict with Unity's real device ids (they start 1,2,3..)
                var deviceType = isFirstPlayer ? localPlayerDeviceType : SupportedInputType.Mouse;
                var playerData = players[i];
                playbackPlayersJoinedModels.Add(new PlayerJoinedModel(playerData.Name, deviceType, deviceId));
            }

            return playbackPlayersJoinedModels;
        }

        private int GetFirstLocalPlayerConnectedDeviceId(out SupportedInputType localPlayerDeviceType)
        {
            var realLocalPlayerDeviceId = 0;
            localPlayerDeviceType = SupportedInputType.Mouse;
            
            var allConnectGamepads = _nativeInputDeviceService.GetAllConnectedGamepads();
            if (allConnectGamepads.Count > 0)
            {
                realLocalPlayerDeviceId = allConnectGamepads[0].deviceId;
                localPlayerDeviceType = SupportedInputType.Gamepad;
            }
            else 
            {
                var allConnectedKeyboards = _nativeInputDeviceService.GetAllConnectedKeyboards();
                if (allConnectedKeyboards.Count > 0)
                {
                    realLocalPlayerDeviceId = allConnectedKeyboards[0].deviceId;
                }
                else
                {
                    LogService.LogError($"No device connected for local player to play!");
                }
            }
            
            return realLocalPlayerDeviceId;
        }

        private void SaveLocallyChosenParameters()
        {
            _dataPersistence.Save(
                PREFS_PLAYERS_JOINED_KEY, _playerJoinedModels,
                PREFS_IP_ADDRESS_KEY, _uiView.IpAddress, 
                PREFS_IS_LOCAL_HOST_KEY, _uiView.IsLocalHost,
                PREFS_PORT_HOST_KEY, _uiView.Port);
        }

        private void OnPlayerNameChanged(int playerIndex, string playerName)
        {
            _playerJoinedModels[playerIndex].PlayerName = playerName;
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