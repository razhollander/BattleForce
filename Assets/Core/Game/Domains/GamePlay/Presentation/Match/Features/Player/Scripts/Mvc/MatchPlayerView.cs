using System;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Simple_Health_Bar.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Scripts.Extensions;
using Core.Scripts.Helpers;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerView : MonoBehaviour, IPoolable
    {
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private SimpleHealthBar _healthBar; 
        [SerializeField] private GameObject _healthBarGameObject; 
        [SerializeField] private GameObject _aimArrowTransform; 
        [SerializeField] private GameObject _moveAssistArrowTransform; 
        [SerializeField] private SpriteRenderer _moveAssistArrowSpriteRenderer; 
        [SerializeField] private Transform _assistArrowParentTransform; 
        [SerializeField] private SpriteAnimator _sentryGunAnimator;
        [SerializeField] private Canvas _spinnedEyesCanvas;
        [SerializeField] private UIImageAnimator _spinnedEyesAnimator;
        [SerializeField] private UmbrellaStickView _umbrellaStickView;
        [SerializeField] private PlayerChickenView _playerChickenView;
        [SerializeField] private YearsOfPainView _yearsOfPainView;
        [SerializeField] private SonicSnapEffectView _sonicSnapEffectView;
        [SerializeField] private GameObject _deadAura;
        [SerializeField] private PlayerEyesView _playerEyesView;
        [SerializeField] private MatchPlayerTalentsHudView _talentsHudView;
        [SerializeField] private MatchPlayerPowerUpHudView _powerUpHudView;
        [SerializeField] private GameObject _crownGameObject;
        [SerializeField] private DeadTombstoneView _deadTombstoneView;
        public Action Despawn { get; set; }
        
        public PlayerView Base => _playerView;
        public void UpdateTalents(TalentVisualData[] talents)
        {
            _talentsHudView.UpdateTalents(talents);
        }

        public void UpdateTalentCooldown(int talentIndex, float maxCooldown, float cooldownLeft, bool isOnCooldown)
        {
            _talentsHudView.UpdateTalentCooldown(talentIndex, maxCooldown, cooldownLeft, isOnCooldown);
        }

        public void UpdateTalentStocks(int talentIndex, int stockAmount)
        {
            _talentsHudView.UpdateTalentStocks(talentIndex, stockAmount);
        }
        
        public void SetSelectedTalent(int selectedTalentIndex, CancellationToken cancellationToken)
        {
            _talentsHudView.SelectTalent(selectedTalentIndex, cancellationToken);
        }
        
        public void MakeAngryForShortDuration(CancellationToken cancellationToken)
        {
            _playerEyesView.MakeAngryForShortDuration(cancellationToken);
        }

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

        public void SetColor(Color color)
        {
            Base.SetColor(color);
            _moveAssistArrowSpriteRenderer.color = color.Darken(0.3f);
        }
        
        public void UpdateHealthBar(int health, int maxHealth, CancellationToken cancellationToken)
        {
            _healthBar.UpdateBar(health, maxHealth, cancellationToken);
        }
        
        public void SetIsSpinned(bool isSpinned, CancellationTokenSource cancellationTokenSource)
        {
            _playerEyesView.SetIsSpinned(isSpinned, cancellationTokenSource);
        }
        
        public void PlayLayEggAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _playerChickenView.PlayLayEggAnimation(cancellationTokenSource).Forget();
        }
        
        public void PlayYearsOfPainAnimation(Vector2 direction, CancellationTokenSource cancellationTokenSource)
        {
            _yearsOfPainView.PlayAndHide(direction, cancellationTokenSource).Forget();
        }

        public void PlaySonicSnapEffect(CancellationToken cancellationToken)
        {
            _sonicSnapEffectView.PlaySnapEffect(cancellationToken).Forget();
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
        
        public void OnDespawned()
        {
            DisableSentryGunState();
            _playerEyesView.OnDespawned();
            _playerChickenView.SetChickenState(false);
            DisableUmbrellaState();
            Base.OnDespawned();
        }

        private void DisableUmbrellaState()
        {
            _umbrellaStickView.HideUmbrella();
        }
        
        public void OnCreated()
        {
            _playerEyesView.OnCreated();
            Base.OnCreated();
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
        public void SetIsDeadEffectEnabled(bool isEnabled, CancellationToken cancellationToken)
        {
            _deadTombstoneView.SetIsShown(isEnabled);
            if (isEnabled)
            {
                _deadTombstoneView.PlayShowAnimation(cancellationToken).Forget();
            }
            
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

        public void OnSpawned()
        {
            SetIsHealthBarShown(true);
            _sonicSnapEffectView.Hide();
            Base.OnSpawned();
        }

        public void SetIsLockOnTargetSightShown(bool isShown)
        {
            Base.SetIsLockOnTargetSightShown(isShown);
            _playerEyesView.UpdateEyesAccordingToIsSightShown(isShown);
        }

        public void SetIsKinged(bool isKinged)
        {
            _crownGameObject.SetActive(isKinged);
        }

        public void SetCurrentPowerUp(bool shouldShowPowerUp, Sprite powerUpIcon)
        {
            _powerUpHudView.SetPowerUp(shouldShowPowerUp, powerUpIcon);
        }
    }
}
