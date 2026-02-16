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

        public Sprite Icon => _talentImage.sprite;

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
        
        public void SetTalent(TalentVisualData talentVisualData)
        {
            _talentImage.sprite = talentVisualData.Icon;
            _canvasGroup.alpha = 1f;

            UpdateCooldown(talentVisualData.MaxCooldown, talentVisualData.CooldownLeft, talentVisualData.IsOnCooldown);
        }
        
        public void UpdateCooldown(float maxCooldown, float cooldownLeft, bool isOnCooldown)
        {
            if (isOnCooldown)
            {
                var progress = cooldownLeft / maxCooldown;
                _cooldownOverlay.enabled = true;
                _cooldownOverlay.fillAmount = Mathf.Clamp01(progress);
                _cooldownText.gameObject.SetActive(true);
                _cooldownText.text = Mathf.CeilToInt(cooldownLeft).ToString();
            }
            else
            {
                _cooldownOverlay.enabled = false;
                _cooldownOverlay.fillAmount = 0;
                _cooldownText.gameObject.SetActive(false);
            }        
        }
    }
}