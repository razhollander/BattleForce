using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI
{
    public class ChooseNetworkRoleUIView : MonoBehaviour
    {
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _hostButton;
        
        private Action _onClientClicked;
        private Action _onHostClicked;

        public void Setup(Action onClientClicked, Action onHostClicked)
        {
            _onClientClicked = onClientClicked;
            _onHostClicked = onHostClicked;
            _clientButton.onClick.AddListener(OnClientClicked);
            _hostButton.onClick.AddListener(OnHostClicked);
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