using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate
{
    public class EnvironmentTeleportGateView : MonoBehaviour
    {
        [SerializeField] private Transform _visuals;
        [SerializeField] private SpriteRenderer _renderer;

        public void SetSize(Vector2 size)
        {
            _visuals.localScale = new Vector3(size.x, size.y, 1f);
        }

        public void Setup(Sprite sprite, Vector2 size)
        {
            _renderer.sprite = sprite;
            _visuals.localScale = new Vector3(size.x, size.y, 1f);
        }

        public void PlayTeleportAnimation()
        {
            _visuals.DOKill();
            _visuals.localScale = Vector2.one;

            _visuals.DOScale(Vector2.one * 1.2f, 0.2f)
                .OnComplete(() =>
                {
                    _visuals.DOScale(Vector2.one, 0.2f);
                });
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
