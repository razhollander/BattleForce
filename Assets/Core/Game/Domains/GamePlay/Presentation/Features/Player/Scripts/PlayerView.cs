using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _availableBulletSpriteRenderer;
    [SerializeField] private SimpleHealthBar _healthBar;
    //[SerializeField] private PlayerLoadingRing playerLoadingRing;

    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }

    public void ShowIsBulletAvailable(bool isAvailable)
    {
        _availableBulletSpriteRenderer.gameObject.SetActive(isAvailable);
    }

    public void UpdateHealthBar(int health, int maxHealth)
    {
        _healthBar.UpdateBar(health, maxHealth);
    }

    public void InterpolateTransform(Vector2 playerPosition, Quaternion playerRotation, float interpolationFactor)
    {
        var lerpedPosition = Vector2.Lerp(transform.position, playerPosition, interpolationFactor);
        var lerpedRotation = Quaternion.Lerp(transform.rotation, playerRotation, interpolationFactor);
        transform.SetPositionAndRotation(lerpedPosition, lerpedRotation);
    }
}
