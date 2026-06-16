using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    public class ChooseNetworkRoleUIView : MonoBehaviour
    {
        [SerializeField] private PlayerJoinedPanelView _playerJoinedPanelViewPrefab;
        
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _serverButton;
        [SerializeField] private Button _playPlaybackButton;
        
        [SerializeField] private Toggle _localHostToggle;
        [SerializeField] private TMP_InputField _ipInputField;
        [SerializeField] private TMP_InputField _portInputField;
        [SerializeField] private TMP_Dropdown _playbacksDropdown;
        [SerializeField] private Transform _playersJoinedPanelsParent;

        private readonly List<PlayerJoinedPanelView> _playerJoinedPanelViews = new List<PlayerJoinedPanelView>();
        private Action _onClientClicked;
        private Action _onHostClicked;
        private Action _onServerClicked;
        private Action _onPlayPlaybackClicked;
        private Action<int, string> _onPlayerNameChanged;
        private Action<int> _onRemovePlayerButtonClicked;

        public TMP_Dropdown PlaybacksDropdown => _playbacksDropdown;

        public string GetSelectedPlayback()
        {
            var selectedOptionIndex = PlaybacksDropdown.value;
            return PlaybacksDropdown.options[selectedOptionIndex].text;
        }
        
        public void Setup(Action onClientClicked, Action onHostClicked, Action onServerClicked, Action onPlayPlaybackClicked, Action<int, string> onPlayerNameChanged, Action<int> onRemovePlayerButtonClicked, bool defaultOnlyLocal, string defaultIp, int defaultPort,List<PlayerJoinedModel> playerJoinedModels)
        {
            _onClientClicked = onClientClicked;
            _onHostClicked = onHostClicked;
            _onServerClicked = onServerClicked;
            _onPlayPlaybackClicked = onPlayPlaybackClicked;
            _onPlayerNameChanged = onPlayerNameChanged;
            _onRemovePlayerButtonClicked = onRemovePlayerButtonClicked;
            _clientButton.onClick.AddListener(OnClientClicked);
            _hostButton.onClick.AddListener(OnHostClicked);
            _serverButton.onClick.AddListener(OnServerClicked);
            _playPlaybackButton.onClick.AddListener(OnPlayPlaybackClicked);

            _localHostToggle.isOn = defaultOnlyLocal;
            _ipInputField.text = defaultIp;
            _portInputField.text = defaultPort.ToString();

            foreach (var playerJoinedModel in playerJoinedModels)
            {
                AddPlayerJoinedPanel(playerJoinedModel.InputDeviceId, playerJoinedModel.PlayerName, playerJoinedModel.PlayerInputType);
            }
            
            _localHostToggle.onValueChanged.AddListener(OnLocalHostToggleChanged);
            OnLocalHostToggleChanged(_localHostToggle.isOn);
        }

        public void AddPlayerJoinedPanel(int inputDeviceId, string playerName, SupportedInputType supportedInputType)
        {
            var playerJoinedPanelView = Instantiate(_playerJoinedPanelViewPrefab, _playersJoinedPanelsParent);
            _playerJoinedPanelViews.Add(playerJoinedPanelView);
            playerJoinedPanelView.Setup(inputDeviceId, playerName, supportedInputType, OnPlayerNameChanged, OnPlayerRemoveButtonClicked);
        }

        private void OnPlayerRemoveButtonClicked(int inputDeviceId)
        {
            _onRemovePlayerButtonClicked?.Invoke(inputDeviceId);
        }

        private void OnPlayerNameChanged(int inputDeviceId, string playerName)
        {
            _onPlayerNameChanged?.Invoke(inputDeviceId, playerName);
        }

        private void OnLocalHostToggleChanged(bool isLocalHost)
        {
            _ipInputField.gameObject.SetActive(!isLocalHost);
        }

        public bool IsLocalHost => _localHostToggle.isOn;
        public string IpAddress => _ipInputField.text;

        public int Port
        {
            get
            {
                if (int.TryParse(_portInputField.text, out var result))
                {
                    return result;
                }
                return 0; // Should handle invalid input, maybe validation or fallback in controller
            }
        }

        private void OnServerClicked()
        {
            _onServerClicked?.Invoke();
        }

        private void OnPlayPlaybackClicked()
        {
            _onPlayPlaybackClicked?.Invoke();
        }

        private void OnClientClicked()
        {
            _onClientClicked?.Invoke();
        }
        
        private void OnHostClicked()
        {
            _onHostClicked?.Invoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void RemovePlayerJoined(int playerInputDeviceId)
        {
            var playerRemoved = _playerJoinedPanelViews.Find(x=>x.InputDeviceId == playerInputDeviceId);
            _playerJoinedPanelViews.Remove(playerRemoved);
            Destroy(playerRemoved.gameObject);
        }
    }
}