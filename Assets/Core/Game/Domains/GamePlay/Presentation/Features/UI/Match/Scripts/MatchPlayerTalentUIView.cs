using Core.Game.Domains.GamePlay.Shared.S2CModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchPlayerTalentUIView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _normalBackground;
        [SerializeField] private Sprite _selectedBackground;
        [SerializeField] private Vector3 _normalScale = Vector3.one;
        [SerializeField] private Vector3 _selectedScale = new Vector3(1.2f, 1.2f, 1.2f);

        public void Setup(Sprite icon)
        {
            if (_icon != null) _icon.sprite = icon;
            if (_cooldownOverlay != null)
            {
                _cooldownOverlay.type = Image.Type.Filled;
                _cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
                _cooldownOverlay.fillAmount = 0;
            }
        }

        public void UpdateView(TalentStateS2C talentState, bool isSelected)
        {
            // Cooldown overlay
            bool isOnCooldown = talentState.IsOnCooldown();
            if (_cooldownOverlay != null)
            {
                if (talentState.MaxCooldown > 0 && isOnCooldown)
                {
                    float progress = talentState.CooldownSecondsLeft / talentState.MaxCooldown;
                    _cooldownOverlay.fillAmount = Mathf.Clamp01(progress);
                }
                else
                {
                    _cooldownOverlay.fillAmount = 0;
                }
            }

            if (_cooldownText != null)
            {
                if (isOnCooldown && talentState.CooldownSecondsLeft > 0)
                {
                    _cooldownText.gameObject.SetActive(true);
                    _cooldownText.text = Mathf.CeilToInt(talentState.CooldownSecondsLeft).ToString();
                }
                else
                {
                    _cooldownText.gameObject.SetActive(false);
                }
            }

            // Selected visual state
            if (_background != null)
            {
                _background.sprite = isSelected ? _selectedBackground : _normalBackground;
            }

            transform.localScale = isSelected ? _selectedScale : _normalScale;
        }
    }
}