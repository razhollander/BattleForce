using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MatchTimerCountdown.Scripts
{
    public class MatchTimerCountdownView : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _secondsLeftText;

        [Header("Last Seconds Tick Animation")]
        [SerializeField] private Color _tickFlashColor = Color.red;
        [SerializeField] private Color _defaultTextColor = Color.white;
        [SerializeField] private float _tickPunchScale = 0.4f;
        [SerializeField] private float _tickPunchDuration = 0.3f;
        [SerializeField] private float _tickColorFadeBackDuration = 0.3f;

        private Sequence _tickSequence;
        private Vector3 _defaultTextScale;
        private bool _hasCachedDefaultScale;

        public void Show()
        {
            _canvas.enabled = true;
        }

        public void SetSecondsLeftText(string secondsLeftText)
        {
            _secondsLeftText.text = secondsLeftText;
        }

        // A punch on the text that flashes it red and eases back to white, played on each of the last countdown ticks.
        public void PlayLastSecondTickAnimation()
        {
            CacheDefaultScaleIfNeeded();

            _tickSequence?.Kill(true);

            var textTransform = _secondsLeftText.transform;
            textTransform.localScale = _defaultTextScale;
            _secondsLeftText.color = _tickFlashColor;

            _tickSequence = DOTween.Sequence()
                .Append(textTransform.DOPunchScale(Vector3.one * _tickPunchScale, _tickPunchDuration))
                .Join(_secondsLeftText.DOColor(_defaultTextColor, _tickColorFadeBackDuration))
                .SetUpdate(true)
                .OnComplete(() => textTransform.localScale = _defaultTextScale);
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }

        private void CacheDefaultScaleIfNeeded()
        {
            if (_hasCachedDefaultScale)
            {
                return;
            }

            _defaultTextScale = _secondsLeftText.transform.localScale;
            _hasCachedDefaultScale = true;
        }
    }
}
