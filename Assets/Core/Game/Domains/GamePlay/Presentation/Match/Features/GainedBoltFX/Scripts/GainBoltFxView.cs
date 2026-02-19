using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FX.Scripts
{
    public class GainBoltFxView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _moveDistance = 1.0f;
        [SerializeField] private float _duration = 1.0f;

        public void Show(int amount, Vector3 position)
        {
            transform.position = position;
            _text.text = $"+{amount}";


            var color = _text.color;
            color.a = 0;
            _text.color = color;
            gameObject.SetActive(true);

            // Animation
            transform.DOMoveY(position.y + _moveDistance, _duration).SetEase(Ease.OutQuad);


            _text.DOFade(1, 0.2f).OnComplete(() => { _text.DOFade(0, 0.2f).SetDelay(_duration - 0.4f).OnComplete(() => { gameObject.SetActive(false); }); });

        }
    }
}
