using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts
{
    public class FrigidBlockView : MonoBehaviour, IPoolable
    {
        [SerializeField] private MeshFilter _meshFilter;

        [Header("Trail")]
        [SerializeField] private MeshFilter _trailMeshFilter;
        [SerializeField] private Transform _trailEmitterA;
        [SerializeField] private Transform _trailEmitterB;
        [Tooltip("Distance (world units) the emitters must travel before a new pair of trail points is emitted.")]
        [SerializeField] private float _pointSpawnDistance = 0.25f;
        [Tooltip("How long (seconds) a trail point lives before it is discarded.")]
        [SerializeField] private float _pointLifetime = 0.6f;
        [Tooltip("Hard cap on live trail point pairs. Bounds the trail length (and memory) when moving fast.")]
        [SerializeField] private int _maxTrailPoints = 64;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public Transform TrailTransform => _trailMeshFilter.transform;
        public Transform TrailEmitterA => _trailEmitterA;
        public Transform TrailEmitterB => _trailEmitterB;
        public Mesh TrailMesh { get; private set; }
        public float PointSpawnDistance => _pointSpawnDistance;
        public float PointLifetime => _pointLifetime;
        public int MaxTrailPoints => _maxTrailPoints;

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

            // One dynamic mesh per view, reused across every spawn from the pool so the trail
            // never allocates a Mesh at gameplay time.
            TrailMesh = new Mesh { name = "FrigidBlockTrailMesh" };
            TrailMesh.MarkDynamic();
            _trailMeshFilter.sharedMesh = TrailMesh;
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
