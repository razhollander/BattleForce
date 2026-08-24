using System.Threading;
using CoreDomain.Scripts.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamBoardUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _gemCountText;
        [SerializeField] private CountableTextView _boltsCountText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameObject _molesKilledContainer;
        [SerializeField] private TextMeshProUGUI _molesKilledCountText;
        [SerializeField] private GameObject _gatePassScoreContainer;
        [SerializeField] private TextMeshProUGUI _gatePassScoreCountText;

        public void Setup(Color teamColor, int teamGems, int teamBolts)
        {
            _backgroundImage.color = teamColor;
            UpdateGems(teamGems);
            _boltsCountText.SetNumber(teamBolts);
        }

        public void SetIsMolesKilledShown(bool isShown)
        {
            _molesKilledContainer.SetActive(isShown);
        }

        public void UpdateMolesKilled(int molesKilled)
        {
            _molesKilledCountText.text = molesKilled.ToString();
        }

        public void SetIsGatePassScoreShown(bool isShown)
        {
            _gatePassScoreContainer.SetActive(isShown);
        }

        public void UpdateGatePassScore(int gatePassScore)
        {
            _gatePassScoreCountText.text = gatePassScore.ToString();
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
