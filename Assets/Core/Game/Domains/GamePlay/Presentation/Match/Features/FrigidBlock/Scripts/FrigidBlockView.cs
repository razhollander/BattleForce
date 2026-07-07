using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts
{
    public class FrigidBlockView : MonoBehaviour, IPoolable
    {
        [SerializeField] private MeshFilter _meshFilter;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
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
