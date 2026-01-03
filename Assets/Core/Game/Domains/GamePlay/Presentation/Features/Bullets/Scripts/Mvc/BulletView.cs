using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public class BulletView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        public void InterpolatePosition(Vector2 position, float lerpFactor)
        {
            var lerpedPosition = Vector2.Lerp(transform.position, position, lerpFactor);
            SetPosition(lerpedPosition);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void SetRadius(float radius)
        {
            var diameter = radius * 2;
            transform.localScale = new Vector3(diameter, diameter, 1);
        }
    }
}