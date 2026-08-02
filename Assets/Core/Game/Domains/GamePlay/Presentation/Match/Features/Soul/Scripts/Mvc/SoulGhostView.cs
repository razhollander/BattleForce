using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc
{
    public class SoulGhostView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer[] _spriteRenderers;
        [SerializeField, Range(0f, 1f)] private float _opacity = 0.5f;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void Setup(Vector2 position, Quaternion rotation, Color teamColor)
        {
            SetTransform(position, rotation);

            foreach (var spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.color = new Color(teamColor.r, teamColor.g, teamColor.b, _opacity);
            }
        }

        public void SetTransform(Vector2 position, Quaternion rotation)
        {
            Transform.SetPositionAndRotation(position, rotation);
        }

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
        }
    }
}
