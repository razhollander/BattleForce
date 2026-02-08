using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs
{
    public class StartMatchButtonView : MonoBehaviour
    {
        [SerializeField] private GameObject _startTextTransform;
        [SerializeField] private TMP_Text _countdownText;

        public void Setup(Vector2 position, float radius)
        {
            transform.position = position;
            SetRadius(radius);
        }

        public void SetStartState()
        {
            _startTextTransform.SetActive(true);
            _countdownText.gameObject.SetActive(false);
        }
        
        public void SetCountdownState()
        {
            _startTextTransform.SetActive(false);
            _countdownText.gameObject.SetActive(true);
        }
        
        public void SetCountdownText(string text)
        {
            _countdownText.text = text;
        }
        
        private void SetRadius(float radius)
        {
            transform.localScale = new Vector3(radius * 2, radius * 2, 1);
        }

        public void SetOpacity(float alpha)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }

            if (_countdownText != null)
            {
                _countdownText.alpha = alpha;
            }

            if (_startTextTransform != null)
            {
                var renderers = _startTextTransform.GetComponentsInChildren<SpriteRenderer>();
                foreach (var renderer in renderers)
                {
                    var color = renderer.color;
                    color.a = alpha;
                    renderer.color = color;
                }

                var texts = _startTextTransform.GetComponentsInChildren<TMP_Text>();
                foreach (var text in texts)
                {
                    text.alpha = alpha;
                }
            }
        }
    }
}
