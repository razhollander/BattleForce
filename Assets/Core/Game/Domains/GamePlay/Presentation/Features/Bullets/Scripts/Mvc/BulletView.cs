using System;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc
{
    public class BulletView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        public void InterpolatePosition(Vector2 position, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(transform.position, position, decay, Time.deltaTime);
            SetPosition(lerpedPosition);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void SetRadius(float radius)
        {
            var diameter = radius * 2;
            transform.localScale = new Vector3(diameter, diameter, 1);
        }

        public void OnCreated()
        {
        }

        public Action Despawn { get; set; }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}