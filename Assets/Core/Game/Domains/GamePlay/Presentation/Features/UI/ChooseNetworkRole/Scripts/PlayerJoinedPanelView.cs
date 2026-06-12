using System;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    public class PlayerJoinedPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _playerNameInputField;
        [SerializeField] private Button _removePlayerButton;
        [SerializeField] private Image _inputImage;
        [SerializeField] private float _imageMaxRadiusMovment;
        [SerializeField] private SerializableDictionary<SupportedInputType, Sprite> _imagePerInputType;

        private int _playerIndex;
        private Action<int, string> _onPlayerNameChanged;
        private Action<int> _onRemovePlayerButtonClicked;

        public void Setup(int playerIndex, string playerName, SupportedInputType supportedInputType, Action<int, string> onPlayerNameChanged, Action<int> onRemovePlayerButtonClicked)
        {
            _playerIndex = playerIndex;
            _playerNameInputField.text = playerName;
            _inputImage.sprite = _imagePerInputType[supportedInputType];
            _onPlayerNameChanged = onPlayerNameChanged;
            _onRemovePlayerButtonClicked = onRemovePlayerButtonClicked;
            _playerNameInputField.onValueChanged.AddListener(OnPlayerNameChanged);
            _removePlayerButton.onClick.AddListener(OnRemovePlayerButtonClicked);
        }

        private void OnRemovePlayerButtonClicked()
        {
            _onRemovePlayerButtonClicked?.Invoke(_playerIndex);
        }

        private void OnPlayerNameChanged(string newName)
        {
            _onPlayerNameChanged.Invoke(_playerIndex, newName);
        }

        public void MoveInputImage(Vector2 delta)
        {
            var newLocalPosition = _inputImage.transform.localPosition.ToVector2XY() + delta;

            if (newLocalPosition.magnitude > _imageMaxRadiusMovment)
            {
                newLocalPosition = newLocalPosition.normalized * _imageMaxRadiusMovment;
            }

            _inputImage.transform.localPosition = newLocalPosition;
        }
    }
}