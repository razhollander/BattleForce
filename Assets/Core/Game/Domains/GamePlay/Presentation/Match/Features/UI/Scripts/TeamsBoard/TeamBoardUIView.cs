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
        [SerializeField] private GameObject _molesHitContainer;
        [SerializeField] private TextMeshProUGUI _molesHitCountText;
        [SerializeField] private GameObject _gatePassScoreContainer;

        public void Setup(Color teamColor, int teamGems, int teamBolts)
        {
            _backgroundImage.color = teamColor;
            UpdateGems(teamGems);
            _boltsCountText.SetNumber(teamBolts);
        }

        public void SetIsMolesHitShown(bool isShown)
        {
            _molesHitContainer.SetActive(isShown);
        }

        public void SetIsGatePassScoreShown(bool isShown)
        {
            _gatePassScoreContainer.SetActive(isShown);
        }

        public void UpdateMolesHit(int molesHit)
        {
            _molesHitCountText.text = molesHit.ToString();
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
