using System;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc
{
    public class SwapFieldView : MonoBehaviour, IPoolable
    {
        private static readonly int SHADER_RADIUS_PROPERTY = Shader.PropertyToID("_Radius");
        private const float SHADER_MAX_RADIUS = 0.5f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private float _spriteRadius;
        private Material _material;

        public void OnCreated()
        {
            _spriteRadius = transform.localScale.x * 0.5f;
            _material = _spriteRenderer.material;
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void SetRadius(float radius)
        {
            var shaderRadius = MathUtils.Remap(0, _spriteRadius, 0,SHADER_MAX_RADIUS, radius);
            _material.SetFloat(SHADER_RADIUS_PROPERTY, shaderRadius);
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