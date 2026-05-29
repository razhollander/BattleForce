using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetEffectView : MonoBehaviour, IPoolable
    {
        private const string LOCK_ON_TARGET_ANIMATION_NAME = "LockOnTarget";
        
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Animation _animation;
        
        public Action Despawn { get; set; }

        public void OnCreated()
        {
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            _animation.Play(LOCK_ON_TARGET_ANIMATION_NAME);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }

        public void Setup(float lockOnTargetDurationInSeconds)
        {
            _animation[LOCK_ON_TARGET_ANIMATION_NAME].speed = 1f/lockOnTargetDurationInSeconds;
        }
        
        public void UpdatePosition(Vector2 lineStartPoint, Vector2 lineEndPoint, Vector2 targetPosition)
        {
            transform.position = targetPosition;
            _lineRenderer.SetPosition(0, lineStartPoint);
            _lineRenderer.SetPosition(1, lineEndPoint);
        }
    }
}
