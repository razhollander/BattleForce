using System.Threading;
using CoreDomain.Scripts.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamBoardUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _gemCountText;
        [SerializeField] private CountableTextView _boltsCountText;
        [SerializeField] private Image _backgroundImage;
        
        public void Setup(Color teamColor, int teamGems, int teamBolts)
        {
            _backgroundImage.color = teamColor;
            UpdateGems(teamGems);
            _boltsCountText.SetNumber(teamBolts);
        }

        public void UpdateGems(int gems)
        {
            _gemCountText.text = gems.ToString();
        }

        public void UpdateBolts(int bolts, CancellationTokenSource cancellationTokenSource, bool immediate = false)
        {
            _boltsCountText.CountToNumber(bolts, cancellationTokenSource, immediate);
        }
    }
}
