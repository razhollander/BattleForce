using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    public class ChooseNetworkRoleUIView : MonoBehaviour
    {
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _serverButton;
        [SerializeField] private Button _playPlaybackButton;
        
        [SerializeField] private Toggle _localHostToggle;
        [SerializeField] private TMP_InputField _ipInputField;
        [SerializeField] private TMP_InputField _portInputField;
        [SerializeField] private TMP_Dropdown _playbacksDropdown;

        private Action _onClientClicked;
        private Action _onHostClicked;
        private Action _onServerClicked;
        private Action _onPlayPlaybackClicked;

        public TMP_Dropdown PlaybacksDropdown => _playbacksDropdown;

        public void Setup(Action onClientClicked, Action onHostClicked, Action onServerClicked, Action onPlayPlaybackClicked, bool defaultOnlyLocal, string defaultIp, int defaultPort)
        {
            _onClientClicked = onClientClicked;
            _onHostClicked = onHostClicked;
            _onServerClicked = onServerClicked;
            _onPlayPlaybackClicked = onPlayPlaybackClicked;
            _clientButton.onClick.AddListener(OnClientClicked);
            _hostButton.onClick.AddListener(OnHostClicked);
            _serverButton.onClick.AddListener(OnServerClicked);
            _playPlaybackButton.onClick.AddListener(OnPlayPlaybackClicked);

            _localHostToggle.isOn = defaultOnlyLocal;
            _ipInputField.text = defaultIp;
            _portInputField.text = defaultPort.ToString();

            _localHostToggle.onValueChanged.AddListener(OnLocalHostToggleChanged);
            OnLocalHostToggleChanged(_localHostToggle.isOn);
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
    }
}