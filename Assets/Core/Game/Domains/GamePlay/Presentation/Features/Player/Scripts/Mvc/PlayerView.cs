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
        [SerializeField] private Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.MatchPlayerTalentUIView _talentViewPrefab;
        [SerializeField] private Transform _talentsContainer;
        private Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.MatchPlayerTalentUIView[] _talentViews;
        [SerializeField] private Transform _leftEyeBall;
        [SerializeField] private Transform _leftEye;
        [SerializeField] private Transform _rightEyeBall;
        [SerializeField] private Transform _rightEye;
        [SerializeField] private float _eyeMovementRadius = 0.1f;
        [SerializeField] private PlayerTailView _tailView;
        [SerializeField] private SpriteAnimator _sentryGunAnimator;
        [SerializeField] private Canvas _spinnedEyesCanvas;
        [SerializeField] private UIImageAnimator _spinnedEyesAnimator;
        [SerializeField] private UmbrellaStickView _umbrellaStickView;
        [SerializeField] private PlayerChickenView _playerChickenView;
        [SerializeField] private YearsOfPainView _yearsOfPainView;
        [SerializeField] private GameObject _deadAura;
        
        private Transform _transform;
        private SpriteRenderer _leftEyeRenderer;
        private SpriteRenderer _rightEyeRenderer;
        private Sprite _defaultLeftEyeSprite;
        private Sprite _defaultRightEyeSprite;
        
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
        
        public void UpdateTalents(Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TalentVisualData[] talents)
        {
            if (_talentViews == null) return;
            for (int i = 0; i < _talentViews.Length; i++)
            {
                var view = _talentViews[i];
                if (i > talents.Length - 1)
                {
                    view.SetNoneTalent();
                }
                else
                {
                    view.SetTalent(talents[i]);
                }
            }
        }

        public void SetSelectedTalent(int selectedTalentIndex)
        {
            if (_talentViews == null) return;
            for (int i = 0; i < _talentViews.Length; i++)
            {
                var talentView = _talentViews[i];
                talentView.SetIsSelected(selectedTalentIndex == i);
            }
        }

        public void UpdateTalentCooldown(int talentIndex, float maxCooldown, float cooldownLeft, bool isOnCooldown)
        {
            if (_talentViews != null && talentIndex < _talentViews.Length)
            {
                _talentViews[talentIndex].UpdateCooldown(maxCooldown, cooldownLeft, isOnCooldown);
            }
        }

        public void UpdateTalentStocks(int talentIndex, int stockAmount)
        {
            if (_talentViews != null && talentIndex < _talentViews.Length)
            {
                _talentViews[talentIndex].SetStocksAmount(stockAmount);
            }
        }

        public void CreateTalents(int maxTalentsAmount)
        {
            if (_talentViews != null || _talentViewPrefab == null || _talentsContainer == null) return;

            _talentViews = new Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.MatchPlayerTalentUIView[maxTalentsAmount];
            for (int i = 0; i < maxTalentsAmount; i++)
            {
                var view = Instantiate(_talentViewPrefab, _talentsContainer);
                view.SetNoneTalent();
                _talentViews[i] = view;
            }
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
            _leftEyeRenderer = _leftEye.GetComponent<SpriteRenderer>();
            _rightEyeRenderer = _rightEye.GetComponent<SpriteRenderer>();
            _defaultLeftEyeSprite = _leftEyeRenderer.sprite;
            _defaultRightEyeSprite = _rightEyeRenderer.sprite;
        }

        public void SetIsSpinned(bool isSpinned, CancellationTokenSource cancellationTokenSource)
        {
            _leftEyeRenderer.sprite = isSpinned ? null : _defaultLeftEyeSprite;
            _rightEyeRenderer.sprite = isSpinned ? null : _defaultRightEyeSprite;

            if (isSpinned)
            {
                _spinnedEyesCanvas.enabled = true;
                _spinnedEyesAnimator.PlayAnimation(cancellationTokenSource).Forget();   
            }
            else
            {
                DisableSpinned();
            }
        }
        
        private void DisableSpinned()
        {
            _spinnedEyesAnimator.StopAnimation();
            _spinnedEyesCanvas.enabled = false;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            SetIsHealthBarShown(true);
        }

        public void OnDespawned()
        {
            DisableSentryGunState();
            DisableSpinned();
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

            UpdateEyesToLookAtAimArrow(rotation);
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
