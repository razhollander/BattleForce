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
        
        [SerializeField] private Toggle _localHostToggle;
        [SerializeField] private TMP_InputField _ipInputField;
        [SerializeField] private TMP_InputField _portInputField;
        [SerializeField] private TMP_InputField _playerNameInputField;

        private Action _onClientClicked;
        private Action _onHostClicked;
        private Action _onServerClicked;

        public void Setup(Action onClientClicked, Action onHostClicked, Action onServerClicked, bool defaultOnlyLocal, string defaultIp, int defaultPort)
        {
            _onClientClicked = onClientClicked;
            _onHostClicked = onHostClicked;
            _onServerClicked = onServerClicked;
            _clientButton.onClick.AddListener(OnClientClicked);
            _hostButton.onClick.AddListener(OnHostClicked);
            _serverButton.onClick.AddListener(OnServerClicked);

            _localHostToggle.isOn = defaultOnlyLocal;
            _ipInputField.text = defaultIp;
            _portInputField.text = defaultPort.ToString();
            _playerNameInputField.text = "Player_" + UnityEngine.Random.Range(1000, 9999);

            _localHostToggle.onValueChanged.AddListener(OnLocalHostToggleChanged);
            OnLocalHostToggleChanged(_localHostToggle.isOn);
        }

        private void OnLocalHostToggleChanged(bool isLocalHost)
        {
            _ipInputField.gameObject.SetActive(!isLocalHost);
        }

        public bool IsLocalHost => _localHostToggle.isOn;
        public string IpAddress => _ipInputField.text;
        public string PlayerName => _playerNameInputField.text;

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