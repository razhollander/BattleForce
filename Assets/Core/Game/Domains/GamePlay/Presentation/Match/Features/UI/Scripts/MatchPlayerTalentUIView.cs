using Core.Game.Domains.GamePlay.Shared.S2CModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerTalentUIView : MonoBehaviour
    {
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _normalBackground;
        [SerializeField] private Sprite _selectedBackground;
        [SerializeField] private Image _talentImage;
        [SerializeField] private Vector3 _normalScale = Vector3.one;
        [SerializeField] private Vector3 _selectedScale = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private Sprite _noneTalentSprite;
        [SerializeField] private CanvasGroup _canvasGroup;

        public void SetNoneTalent()
        {
            _talentImage.sprite = _noneTalentSprite;
            _canvasGroup.alpha = 0.5f;
            _cooldownText.gameObject.SetActive(false);
        }
        
        public void SetIsSelected(bool isSelected)
        {
            _background.sprite = isSelected ? _selectedBackground : _normalBackground;
            _talentImage.transform.localScale = isSelected ? _selectedScale : _normalScale;
        }
        
        public void SetupTalent(Sprite icon)
        {
            _cooldownText.gameObject.SetActive(true);
            _talentImage.sprite = icon;
            _canvasGroup.alpha = 1f;
            _cooldownOverlay.enabled = false;
        }

        public void UpdateView(TalentStateS2C talentState, bool isSelected)
        {
            bool isOnCooldown = talentState.IsOnCooldown();

            if (talentState.MaxCooldown > 0 && isOnCooldown)
            {
                float progress = talentState.CooldownSecondsLeft / talentState.MaxCooldown;
                _cooldownOverlay.enabled = true;
                _cooldownOverlay.fillAmount = Mathf.Clamp01(progress);
            }
            else
            {
                _cooldownOverlay.enabled = false;
                _cooldownOverlay.fillAmount = 0;
            }

            if (isOnCooldown && talentState.CooldownSecondsLeft > 0)
            {
                _cooldownText.gameObject.SetActive(true);
                _cooldownText.text = Mathf.CeilToInt(talentState.CooldownSecondsLeft).ToString();
            }
            else
            {
                _cooldownText.gameObject.SetActive(false);
            }

            _background.sprite = isSelected ? _selectedBackground : _normalBackground;

            _talentImage.transform.localScale = isSelected ? _selectedScale : _normalScale;
        }
    }
}