using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public class KOProjectileView : MonoBehaviour , IPoolable
    {
        [SerializeField] private KOProjectileCoilSpringView _coilSpringView;
        public Transform Transform;

        public void Setup(Vector2 position, Quaternion rotation, Vector2 coilSpringStartPosition, float size)
        {
            Transform.localScale = new Vector3(size, size, 1);
            SetTransform(position, rotation, coilSpringStartPosition);
        }
        
        public void SetTransform(Vector2 position, Quaternion rotation, Vector2 coilSpringStartPosition)
        {
            Transform.SetPositionAndRotation(position, rotation);
            _coilSpringView.UpdateEndPoints(position, coilSpringStartPosition);
        }
        
        public void OnCreated()
        {
            Transform = transform;
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
