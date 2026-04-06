using System;
using TMPro;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing;
using Core.Game.Domains.GamePlay.Presentation.Features.Simple_Health_Bar.Scripts;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerView : MonoBehaviour, IPoolable
    {
        private static readonly int TAIL_BEND_SHADER_PROPERTY = Shader.PropertyToID("_Bend");

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _availableBulletSpriteRenderer;
        [SerializeField] private SimpleHealthBar _healthBar; // todo move to the match domain
        [SerializeField] private GameObject _healthBarGameObject; // todo move to the match domain
        [SerializeField] private PlayerLoadingRingView _loadingRingView;
        [SerializeField] private Transform _spaceShipTransform;
        [SerializeField] private Transform _aimArrowTransform; // todo move to the match domain
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private Image _selectedTalentImage; // todo move to the match domain
        [SerializeField] private Transform _leftEyeBall;
        [SerializeField] private Transform _leftEye;
        [SerializeField] private Transform _rightEyeBall;
        [SerializeField] private Transform _rightEye;
        [SerializeField] private float _eyeMovementRadius = 0.1f;
        [SerializeField] private PlayerTailView _tailView;
        
        private Transform _transform;
        private Material _tailMaterial;

        public Action Despawn { get; set; }

        public void InitEntryPoint()
        {
            _tailView.InitEntryPoint();
        }
        
        public void SetTalentSprite(Sprite sprite)
        {
            _selectedTalentImage.sprite = sprite;
        }

        public void SetPlayerName(string playerName)
        {
            _playerNameText.text = playerName;
        }

        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
            _tailView.SetColor(color);
            _availableBulletSpriteRenderer.color = color;
        }

        public void SetBulletLoading(float cooldownLeft, float maxCooldown)
        {
            _loadingRingView.SetRingScale(cooldownLeft/maxCooldown);
        }

        public void SetTalentLoading(float cooldownLeft, float maxCooldown)
        {
            _loadingRingView.SetRingArc(cooldownLeft, maxCooldown);
        }
        
        public void SetPositionAndRotation(Vector2 position, Quaternion rotation)
        {
            transform.position = position;
            _spaceShipTransform.rotation = rotation;
        }

        public void ShowIsBulletAvailable(bool isAvailable)
        {
            _availableBulletSpriteRenderer.gameObject.TrySetActive(isAvailable);
        }

        public void UpdateHealthBar(int health, int maxHealth)
        {
            _healthBar.UpdateBar(health, maxHealth);
        }

        public void InterpolateTransform(Vector2 playerPosition, Quaternion playerRotation, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(transform.position, playerPosition, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(_spaceShipTransform.rotation, playerRotation, decay, Time.deltaTime);
            SetPositionAndRotation(lerpedPosition, lerpedRotation);
        }

        public Vector2 GetPosition()
        {
            return _spaceShipTransform.position;
        }

        public void OnCreated()
        {
            _transform = transform;
            _loadingRingView.InitEntryPoint();
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            SetIsHealthBarShown(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
        
        public Transform GetSpaceShipTransform()
        {
            return _spaceShipTransform;
        }
        
        public Transform GetTransform()
        {
            return _transform;
        }

        public void SetIsHealthBarShown(bool isShown)
        {
            _healthBarGameObject.SetActive(isShown);
        }

        public void InterpolateAimRotation(System.Numerics.Vector2 direction, float decay)
        {
            if (direction.LengthSquared() < 0.0001f)
            {
                LogService.LogError("Direction is too small (0) to interpolate");

                return;
            }

            var targetRotation = direction.ToQuaternion();

            _aimArrowTransform.rotation = MathUtils.ExpDecay(
                _aimArrowTransform.rotation,
                targetRotation,
                decay,
                Time.deltaTime
            );

            UpdateEyesToLookAtAimArrow(direction);
        }

        private void UpdateEyesToLookAtAimArrow(System.Numerics.Vector2 aimArrowDirection)
        {
            var eyeOffset = new Vector2(aimArrowDirection.X, aimArrowDirection.Y).normalized * _eyeMovementRadius;
            var leftPosition = _leftEye.position.ToVector2XY() + eyeOffset;
            var rightPosition = _rightEye.position.ToVector2XY() + eyeOffset;
            _leftEyeBall.position = new Vector3(leftPosition.x, leftPosition.y, _leftEyeBall.position.z);
            _rightEyeBall.position = new Vector3(rightPosition.x, rightPosition.y, _rightEyeBall.position.z);
        }
    }
}
