using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts
{
    public class SecondCastAimArrowView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Transform _arrowTransform;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public Action Despawn { get; set; }

        public void Setup(Vector2 position, Quaternion rotation)
        {
            SetTransform(position, rotation);
        }

        public void SetTransform(Vector2 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
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
    }
}
