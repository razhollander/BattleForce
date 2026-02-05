using System;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing;
using Core.Game.Domains.GamePlay.Presentation.Features.Simple_Health_Bar.Scripts;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _availableBulletSpriteRenderer;
        [SerializeField] private SimpleHealthBar _healthBar; // todo move to the match domain
        [SerializeField] private GameObject _healthBarGameObject; // todo move to the match domain
        [SerializeField] private PlayerLoadingRing _playerLoadingRing;
        [SerializeField] private Transform _spaceShipTransform;

        public Action Despawn { get; set; }

        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
            _availableBulletSpriteRenderer.color = color;
        }

        public void InterpolateBulletLoading(float cooldownLeft, float maxCooldown, float lerpFactor)
        {
            _playerLoadingRing.SetRingScale(cooldownLeft/maxCooldown, lerpFactor);
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

        public void InterpolateTransform(Vector2 playerPosition, Quaternion playerRotation, float lerpFactor)
        {
            var lerpedPosition = Vector2.Lerp(transform.position, playerPosition, lerpFactor);
            var lerpedRotation = Quaternion.Lerp(_spaceShipTransform.rotation, playerRotation, lerpFactor);
            SetPositionAndRotation(lerpedPosition, lerpedRotation);
        }

        public Vector2 GetPosition()
        {
            return _spaceShipTransform.position;
        }

        public void OnCreated()
        {
        }
        
        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
        
        public Transform GetTransform()
        {
            return _spaceShipTransform;
        }

        public void HidePlayerHealthBar()
        {
            _healthBarGameObject.SetActive(false);
        }
    }
}
