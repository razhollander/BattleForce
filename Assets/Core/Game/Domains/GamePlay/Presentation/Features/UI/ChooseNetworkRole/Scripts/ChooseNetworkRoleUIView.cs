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
        
        private Action _onClientClicked;
        private Action _onHostClicked;
        private Action _onServerClicked;
        private Action _onPlayFabClicked;

        public void Setup(Action onClientClicked, Action onHostClicked, Action onServerClicked, Action onPlayFabClicked)
        {
            _onClientClicked = onClientClicked;
            _onHostClicked = onHostClicked;
            _onServerClicked = onServerClicked;
            _onPlayFabClicked = onPlayFabClicked;
            _clientButton.onClick.AddListener(OnClientClicked);
            _hostButton.onClick.AddListener(OnHostClicked);
            _serverButton.onClick.AddListener(OnServerClicked);

            CreatePlayFabButton();
        }

        private void CreatePlayFabButton()
        {
            // Instantiate existing button to copy style
            var playFabButton = Instantiate(_clientButton, _clientButton.transform.parent);
            playFabButton.name = "PlayFabButton";

            // Set Text
            var tmpText = playFabButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = "Join PlayFab";
            }

            // Handle Position
            playFabButton.transform.SetAsLastSibling();

            // If no layout group, we need to manually offset it
            if (playFabButton.transform.parent.GetComponent<LayoutGroup>() == null)
            {
                var rt = playFabButton.GetComponent<RectTransform>();
                var clientRt = _clientButton.GetComponent<RectTransform>();
                // Move it down by assumed height + padding
                // Use host and client difference if possible, but _hostButton might be anywhere
                // Let's just offset by height * 1.5
                float offset = clientRt.rect.height * 1.5f;
                // Check if we should go down relative to the last button (Server or Host)
                // Let's put it below Server button if possible
                var lastBtnRt = _serverButton.GetComponent<RectTransform>();
                if (lastBtnRt != null)
                {
                    rt.anchoredPosition = lastBtnRt.anchoredPosition + new Vector2(0, -offset);
                }
                else
                {
                    rt.anchoredPosition = clientRt.anchoredPosition + new Vector2(0, -offset * 3);
                }
            }

            // Setup Listener
            playFabButton.onClick.RemoveAllListeners();
            playFabButton.onClick.AddListener(OnPlayFabClicked);
        }

        private void OnPlayFabClicked()
        {
            _onPlayFabClicked?.Invoke();
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
