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

        private CancellationTokenSource _boltsCancellationTokenSource = new();

        public void Setup(Color teamColor)
        {
            _backgroundImage.color = teamColor;
            UpdateGems(0);
            _boltsCountText.SetNumber(0);
        }

        public void UpdateGems(int gems)
        {
            _gemCountText.text = gems.ToString();
        }

        public void UpdateBolts(int bolts, bool immediate = false)
        {
            _boltsCancellationTokenSource.Cancel();
            _boltsCancellationTokenSource.Dispose();
            _boltsCancellationTokenSource = new CancellationTokenSource();
            _boltsCountText.CountToNumber(bolts, _boltsCancellationTokenSource, immediate);
        }

        private void OnDestroy()
        {
            _boltsCancellationTokenSource.Cancel();
            _boltsCancellationTokenSource.Dispose();
        }
    }
}
