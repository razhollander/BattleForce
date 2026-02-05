using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamBoardUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _gemCountText;
        [SerializeField] private Image _backgroundImage;

        public void Setup(Color teamColor)
        {
            _backgroundImage.color = teamColor;
            UpdateGems(0);
        }

        public void UpdateGems(int gems)
        {
            _gemCountText.text = gems.ToString();
        }
    }
}
