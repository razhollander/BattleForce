using System.Collections;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCircle : MonoBehaviour
{
    private const int MaxHealth = 5;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _velocity = 5f;
    [SerializeField] private ArrowButtons _arrowsButtons;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private Bullet _bullet;
    [SerializeField] private PlayerLoadingRing playerLoadingRing;
    [SerializeField] private int _health = 5;
    [SerializeField] private SimpleHealthBar _healthBar;
    [SerializeField] private GameObject _healthBarGO;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private ScoreService _scoreService;
    [SerializeField] private Color _playerColor;
    
    private Rigidbody2D _rigidbody;
    public Vector2 LookDirection { get; private set; }
    public bool IsDead { get; private set; }
    public int ID;
    private void Awake()
    {
        SetArrowsColor();
        SetScoreColor();
        SetSelfColor();
        
        _rigidbody = GetComponent<Rigidbody2D>();
        var x = Random.Range(-1f, 1f);
        var y = Random.Range(-1f, 1f);
        SetRotation(new Vector2(x, y).normalized);
        _healthBar.UpdateBar(_health, MaxHealth);
        
        if (_arrowsButtons != null)
        {
            _arrowsButtons.RightButton.OnPress+=(OnRightClick);
            _arrowsButtons.LeftButton.OnPress+=(OnLeftClick);
            _arrowsButtons.DownButton.OnPointerDownEvent+=(OnDownClick);
            _arrowsButtons.UpButton.OnPointerDownEvent+=(OnUpButtonClicked);
        }
    }

    private void SetSelfColor()
    {
        _spriteRenderer.color = _playerColor;
        _bullet.SetColor(_playerColor);
    }

    private void SetScoreColor()
    {
        _scoreService.SetColorForPlayer(ID, _playerColor);
    }

    private void SetArrowsColor()
    {
        _arrowsButtons.SetColor(_playerColor);
    }

    public void SetRotation(Vector2 rotationDirection)
    {
        LookDirection = rotationDirection;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, LookDirection);
    }

    private void OnDownClick()
    {
        // if (playerLoadingRing.IsBulletLoadingReady)
        // {
        //     var newBulletGO = Instantiate(_bullet.gameObject);
        //     newBulletGO.transform.position = _bullet.transform.position;
        //     var newBullet = newBulletGO.GetComponent<Bullet>();
        //     newBullet.Fire(LookDirection);
        //     _bullet.gameObject.SetActive(false);
        //     playerLoadingRing.DoBulletLoading(ShowBulletBack);
        // }
    }

    private void ShowBulletBack()
    {
        _bullet.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if(IsDead) return;
        
        _rigidbody.MovePosition(transform.position.ToVector2XY()+LookDirection * (_velocity * Time.fixedDeltaTime));
    }
    
    private void OnLeftClick()
    {
        SetRotation(LookDirection.Rotate(_rotationSpeed));
    }

    private void OnRightClick()
    {
        SetRotation(LookDirection.Rotate(-_rotationSpeed));
    }

    public void Hit()
    {
        if(IsDead) return;
        
        DecrementHealth();
    }

    private void DecrementHealth()
    {
        _health--;
        _healthBar.UpdateBar(_health, MaxHealth);
        
        if (_health == 0)
        {
            Die();
        }
    }

    [ContextMenu("Die")]
    private void Die()
    {
        // IsDead = true;
        // _rigidbody.bodyType = RigidbodyType2D.Static;
        // _healthBarGO.SetActive(false);
        // playerLoadingRing.BulletLoadingTime = 2f;
        // _scoreService.IncrementAllPlayersAliveScore();
        // CheckForWinner();
    }

    private void OnUpButtonClicked()
    {
        // if(!playerLoadingRing.IsPowerUpLoadingReady) return;
        //
        // var closetPayer = GetClosetPayer();
        //
        // SwapPositionWithPlayer(closetPayer);
        // SwapLookDirectionWithPlayer(closetPayer);
        // playerLoadingRing.DoPowerUpLoading();
    }

    private void SwapLookDirectionWithPlayer(PlayerCircle closetPayer)
    {
        var closetPlayerDirection = closetPayer.LookDirection;
        closetPayer.SetRotation(LookDirection);
        SetRotation(closetPlayerDirection);
    }

    private void SwapPositionWithPlayer(PlayerCircle closetPayer)
    {
        (closetPayer.transform.position, transform.position) = (transform.position, closetPayer.transform.position);
    }

    private PlayerCircle GetClosetPayer()
    {
        PlayerCircle closetPayer = null;
        var players = FindObjectsOfType<PlayerCircle>();
        var minDistance = float.MaxValue;

        foreach (var player in players)
        {
            if (player.ID != ID)
            {
                var distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closetPayer = player;
                }
            }
        }

        return closetPayer;
    }

    private void CheckForWinner()
    {
        var players = FindObjectsOfType<PlayerCircle>();
        var numberOfPlayersAlive = players.Count(x => !x.IsDead);
        
        if (numberOfPlayersAlive == 1)
        {
            var winner = players.First(x => !x.IsDead);
            StartCoroutine(EndMatch(winner));
        }
    }

    private IEnumerator EndMatch(PlayerCircle winner)
    {
        Time.timeScale = 0;
        _winnerText.color = winner._playerColor;
        _winnerText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(3);
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
        Time.timeScale = 1;
    }
}
