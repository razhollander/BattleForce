using UnityEngine;
using DG.Tweening;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.EnvironmentTeleportGate
{
    public class EnvironmentTeleportGateView : MonoBehaviour
    {
        [SerializeField] private Transform _visuals;
        [SerializeField] private SpriteRenderer _renderer;

        private Vector3 _originalScale;

        public void SetSize(Vector2 size)
        {
            _visuals.localScale = new Vector3(size.x, size.y, 1f);
            _originalScale = _visuals.localScale;
        }

        public void SetColor(Color color)
        {
            if (_renderer != null)
            {
                _renderer.color = color;
            }
        }

        public void PlayTeleportAnimation()
        {
            _visuals.DOKill();
            _visuals.localScale = _originalScale;

            _visuals.DOScale(_originalScale * 1.2f, 0.1f)
                .OnComplete(() =>
                {
                    _visuals.DOScale(_originalScale, 0.1f);
                });
        }
    }
}
