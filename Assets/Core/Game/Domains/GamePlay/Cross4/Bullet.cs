using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private float _speed = 10;
    [SerializeField] private GameObject _parentPlayer;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    public void Fire(Vector2 lookDirection)
    {
        _rigidbody.simulated = true;
        _rigidbody.linearVelocity = lookDirection.normalized * _speed;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject != _parentPlayer)
        {
            Destroy(gameObject);
           
            var playerHit = collision.collider.GetComponent<PlayerCircle>();
            
            if (playerHit != null)
            {
                playerHit.Hit();
            }
        }
    }

    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }
}
