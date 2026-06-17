using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WaterGunStream.Scripts.Mvc
{
    public class WaterGunStreamView : MonoBehaviour, IPoolable
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private ParticleSystem _splashParticles;

        private static readonly int BendAmountProperty = Shader.PropertyToID("_BendAmount");

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void OnCreated()
        {
            Transform = transform;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
            if (_splashParticles != null)
            {
                _splashParticles.Stop();
            }
        }

        public void Setup(Vector2 position)
        {
            Transform.position = new Vector3(position.x, position.y, Transform.position.z);
        }

        public void UpdateStream(Vector2 aimDirection, float angularVelocity)
        {
            var angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            Transform.rotation = Quaternion.Euler(0f, 0f, angle);

            _meshRenderer.material.SetFloat(BendAmountProperty, angularVelocity);
        }
    }
}
