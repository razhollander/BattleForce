using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WhacAMoleCountdown.Scripts
{
    public class WhacAMoleCountdownView : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _secondsLeftText;

        public void SetSecondsLeftText(string secondsLeftText)
        {
            _canvas.enabled = true;
            _secondsLeftText.text = secondsLeftText;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }
    }
}
