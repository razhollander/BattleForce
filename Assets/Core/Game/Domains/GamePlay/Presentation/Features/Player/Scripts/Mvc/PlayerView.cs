using System;
using TMPro;
using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTargetSight;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _availableBulletSpriteRenderer;
        [SerializeField] private PlayerLoadingRingView _loadingRingView;
        [SerializeField] private Transform _spaceShipTransform;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private PlayerTailView _tailView;
        [SerializeField] private Transform _heart;
        [SerializeField] private LockOnTargetSightView _lockOnTargetSightView;

        private Transform _transform;
        
        public Action Despawn { get; set; }

        
        public void UpdateTailBend()
        {
            _tailView.UpdateTail();
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
            _heart.rotation = Quaternion.identity;
        }

        public void ShowIsBulletAvailable(bool isAvailable)
        {
            _availableBulletSpriteRenderer.gameObject.TrySetActive(isAvailable);
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
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void SetIsLockOnTargetSightShown(bool isShown)
        {
            _lockOnTargetSightView.SetIsShown(isShown);
        }

        public void OnDespawned()
        {
            SetIsLockOnTargetSightShown(false);
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

        public Transform GetHeartTransform()
        {
            return _heart;
        }
        
        public void SetIsTailWaving(bool isWaving)
        {
            _tailView.SetIsTailWaving(isWaving);
        }

        public Transform GetHeadTransform()
        {
            return _headTransform;
        }
    }
}