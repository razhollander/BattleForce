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
        [SerializeField] private MatchPlayerTalentStockUIView _stockView;
        [SerializeField] private Color _cooldownOverlayColor = Color.white;
        [SerializeField] private Color _cooldownOverlayColorWhenOnCooldown = Color.black;

        public void SetNoneTalent()
        {
            _talentImage.sprite = _noneTalentSprite;
            _canvasGroup.alpha = 0.5f;
            _cooldownText.gameObject.SetActive(false);
        }

        public void SetAreEnabledStocks(bool areEnabled)
        {
            _stockView.gameObject.SetActive(areEnabled);
        }

        public void SetStocksAmount(int amount)
        {
            _stockView.SetStockAmount(amount);
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
            var isStockable = talentVisualData.IsStockable;
            SetAreEnabledStocks(isStockable);

            if (isStockable)
            {
                SetStocksAmount(talentVisualData.StocksAmount);
            }
        }
        
        public void UpdateCooldown(float maxCooldown, float cooldownLeft, bool isOnCooldown)
        {
            if (cooldownLeft > 0)
            {
                var progress = cooldownLeft / maxCooldown;
                _cooldownOverlay.enabled = true;
                _cooldownOverlay.fillAmount = progress;
                _cooldownText.gameObject.SetActive(true);
                var text = cooldownLeft < 1 ? cooldownLeft.ToString("F2") : Mathf.FloorToInt(cooldownLeft).ToString();
                _cooldownText.text = text;
            }
            else
            {
                _cooldownOverlay.enabled = false;
                _cooldownOverlay.fillAmount = 0;
                _cooldownText.gameObject.SetActive(false);
            }        
            
            _cooldownOverlay.color = isOnCooldown ? _cooldownOverlayColorWhenOnCooldown: _cooldownOverlayColor;
        }
    }
}