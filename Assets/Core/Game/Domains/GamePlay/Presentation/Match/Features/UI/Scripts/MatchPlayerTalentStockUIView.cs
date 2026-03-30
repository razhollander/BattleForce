using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerTalentStockUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _backgroundZeroStockColor;
        [SerializeField] private Color _backgroundDefaultColor;
        
        public void SetStockAmount(int amount)
        {
            _text.text = amount.ToString();
            _backgroundImage.color = amount > 0 ? _backgroundDefaultColor : _backgroundZeroStockColor;
        }
    }
}
