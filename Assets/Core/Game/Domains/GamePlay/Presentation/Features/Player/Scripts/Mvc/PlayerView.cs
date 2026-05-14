using System;
using System.Threading;
using TMPro;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing;
using Core.Game.Domains.GamePlay.Presentation.Features.Simple_Health_Bar.Scripts;
using Core.Scripts.Extensions;
using Core.Scripts.Helpers;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _availableBulletSpriteRenderer;
        [SerializeField] private SimpleHealthBar _healthBar; // todo move to the match domain
        [SerializeField] private GameObject _healthBarGameObject; // todo move to the match domain
        [SerializeField] private PlayerLoadingRingView _loadingRingView;
        [SerializeField] private Transform _spaceShipTransform;
        [SerializeField] private GameObject _aimArrowTransform; // todo move to the match domain
        [SerializeField] private GameObject _moveAssistArrowTransform; // todo move to the match domain
        [SerializeField] private SpriteRenderer _moveAssistArrowSpriteRenderer; // todo move to the match domain
        [SerializeField] private Transform _assistArrowParentTransform; // todo move to the match domain
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private Image _selectedTalentImage; // todo move to the match domain
        [SerializeField] private PlayerEyesView _playerEyesView;
        [SerializeField] private PlayerTailView _tailView;
        [SerializeField] private SpriteAnimator _sentryGunAnimator;
        [SerializeField] private UmbrellaStickView _umbrellaStickView;
        [SerializeField] private PlayerChickenView _playerChickenView;
        [SerializeField] private YearsOfPainView _yearsOfPainView;
        [SerializeField] private GameObject _deadAura;
        [SerializeField] private Transform _heart;
        
        private Transform _transform;
        
        public Action Despawn { get; set; }

        public void SetSentryGunState(bool isOn, CancellationTokenSource cancellationTokenSource)
        {
            if (isOn)
            {
                _sentryGunAnimator.gameObject.TrySetActive(true);
                _sentryGunAnimator.PlayAnimation(cancellationTokenSource).Forget();
            }
            else
            {
                DisableSentryGunState();
            }
        }

        private void DisableSentryGunState()
        {
            _sentryGunAnimator.StopAnimation();
            _sentryGunAnimator.gameObject.TrySetActive(false);
        }

        public void SetChickenState(bool isOn)
        {
            _playerChickenView.SetChickenState(isOn);
        }
        
        public void PlayLayEggAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _playerChickenView.PlayLayEggAnimation(cancellationTokenSource).Forget();
        }
        
        public void PlayYearsOfPainAnimation(Vector2 direction, CancellationTokenSource cancellationTokenSource)
        {
            _yearsOfPainView.PlayAndHide(direction, cancellationTokenSource).Forget();
        }

        public void SetUmbrellaState(bool isOn)
        {
            if (isOn)
            {
                _umbrellaStickView.ShowUmbrella();
            }
            else
            {
                DisableUmbrellaState();
            }
        }

        public void UpdateTailBend()
        {
            _tailView.UpdateTail();
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
            _moveAssistArrowSpriteRenderer.color = color.Darken(0.3f);
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
            _heart.rotation = Quaternion.identity;
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
            _loadingRingView.OnCreated();
            _tailView.OnCreated();
            _playerEyesView.OnCreated();
        }

        public void SetIsSpinned(bool isSpinned, CancellationTokenSource cancellationTokenSource)
        {
            _playerEyesView.SetIsSpinned(isSpinned, cancellationTokenSource);
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            SetIsHealthBarShown(true);
        }

        public void OnDespawned()
        {
            DisableSentryGunState();
            _playerEyesView.OnDespawned();
            DisableUmbrellaState();
            _playerChickenView.SetChickenState(false);
            gameObject.SetActive(false);
        }
        
        private void DisableUmbrellaState()
        {
            _umbrellaStickView.HideUmbrella();
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

            _assistArrowParentTransform.rotation = MathUtils.ExpDecay(
                _assistArrowParentTransform.rotation,
                targetRotation,
                decay,
                Time.deltaTime
            );

            _playerEyesView.UpdateEyesToLookAtDirection(direction);
        }

        public void SetIsTailWaving(bool isWaving)
        {
            _tailView.SetIsTailWaving(isWaving);
        }

        public void InterpolateUmbrellaRotation(System.Numerics.Vector2 rotation, float decay)
        {
            if (rotation.LengthSquared() < 0.0001f)
            {
                LogService.LogError("Direction is too small (0) to interpolate");
                return;
            }

            var targetRotation = rotation.ToQuaternion();

            _umbrellaStickView.SetRotation(MathUtils.ExpDecay(
                _assistArrowParentTransform.rotation,
                targetRotation,
                decay,
                Time.deltaTime
            ));

            _playerEyesView.UpdateEyesToLookAtDirection(rotation);
        }

        public void SetIsDeadAuraEnabled(bool isEnabled)
        {
            _deadAura.SetActive(isEnabled);
        }

        public void ShowMoveAssistArrow()
        {
            _aimArrowTransform.TrySetActive(false);
            _moveAssistArrowTransform.TrySetActive(true);
        }

        public void ShowAimAssistArrow()
        {
            _aimArrowTransform.TrySetActive(true);
            _moveAssistArrowTransform.TrySetActive(false);
        }

        public void HideAssistArrow()
        {
            _aimArrowTransform.TrySetActive(false);
            _moveAssistArrowTransform.TrySetActive(false);
        }
    }
}