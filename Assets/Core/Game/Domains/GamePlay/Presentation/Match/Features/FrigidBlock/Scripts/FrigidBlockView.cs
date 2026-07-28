using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts
{
    public class FrigidBlockView : MonoBehaviour, IPoolable
    {
        private const string TRAIL_MESH_NAME = "FrigidBlockTrailMesh";

        [SerializeField] private MeshFilter _meshFilter;

        [Header("Trail")]
        [SerializeField] private MeshFilter _trailMeshFilter;
        [FormerlySerializedAs("_trailEmitterA")]
        [SerializeField] private Transform _trailLeftEdgeEmitter;
        [FormerlySerializedAs("_trailEmitterB")]
        [SerializeField] private Transform _trailRightEdgeEmitter;
        [Tooltip("Distance (world units) the emitters must travel before a new trail column is emitted.")]
        [FormerlySerializedAs("_pointSpawnDistance")]
        [SerializeField] private float _trailColumnSpawnDistance = 0.25f;
        [Tooltip("How long (seconds) a trail column lives before it is discarded.")]
        [FormerlySerializedAs("_pointLifetime")]
        [SerializeField] private float _trailColumnLifetimeInSeconds = 0.6f;
        [Tooltip("Hard cap on live trail columns. Bounds the trail length (and memory) when moving fast.")]
        [FormerlySerializedAs("_maxTrailPoints")]
        [SerializeField] private int _maxTrailColumns = 64;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public Transform TrailTransform => _trailMeshFilter.transform;
        public Transform TrailLeftEdgeEmitter => _trailLeftEdgeEmitter;
        public Transform TrailRightEdgeEmitter => _trailRightEdgeEmitter;
        public Mesh TrailMesh { get; private set; }
        public float TrailColumnSpawnDistance => _trailColumnSpawnDistance;
        public float TrailColumnLifetimeInSeconds => _trailColumnLifetimeInSeconds;
        public int MaxTrailColumns => _maxTrailColumns;

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
            CreateReusableTrailMesh();
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }

        public void DestroyTrailMesh()
        {
            Destroy(TrailMesh);
            TrailMesh = null;
        }

        private void CreateReusableTrailMesh()
        {
            TrailMesh = new Mesh { name = TRAIL_MESH_NAME };
            TrailMesh.MarkDynamic();
            _trailMeshFilter.sharedMesh = TrailMesh;
        }
    }
}
