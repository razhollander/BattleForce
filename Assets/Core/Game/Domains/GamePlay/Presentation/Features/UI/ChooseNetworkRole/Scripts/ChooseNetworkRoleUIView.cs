using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    public class ChooseNetworkRoleUIView : MonoBehaviour
    {
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _serverButton;
        
        private Action _onClientClicked;
        private Action _onHostClicked;
        private Action _onServerClicked;

        public void Setup(Action onClientClicked, Action onHostClicked, Action onServerClicked)
        {
            _onClientClicked = onClientClicked;
            _onHostClicked = onHostClicked;
            _onServerClicked = onServerClicked;
            _clientButton.onClick.AddListener(OnClientClicked);
            _hostButton.onClick.AddListener(OnHostClicked);
            _serverButton.onClick.AddListener(OnServerClicked);
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