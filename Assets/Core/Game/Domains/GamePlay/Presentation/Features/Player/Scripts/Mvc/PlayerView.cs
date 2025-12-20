using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _availableBulletSpriteRenderer;
        [SerializeField] private SimpleHealthBar _healthBar;
        [SerializeField] private PlayerLoadingRing _playerLoadingRing;
        
        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        public void InterpolateBulletLoading(float cooldownLeft, float maxCooldown, float lerpFactor)
        {
            _playerLoadingRing.SetRingScale(cooldownLeft/maxCooldown, lerpFactor);
        }
        
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        public void ShowIsBulletAvailable(bool isAvailable)
        {
            _availableBulletSpriteRenderer.gameObject.TrySetActive(isAvailable);
        }

        public void UpdateHealthBar(int health, int maxHealth)
        {
            _healthBar.UpdateBar(health, maxHealth);
        }

        public void InterpolateTransform(Vector2 playerPosition, Quaternion playerRotation, float lerpFactor)
        {
            var lerpedPosition = Vector2.Lerp(transform.position, playerPosition, lerpFactor);
            var lerpedRotation = Quaternion.Lerp(transform.rotation, playerRotation, lerpFactor);
            transform.SetPositionAndRotation(lerpedPosition, lerpedRotation);
        }
    }
}
