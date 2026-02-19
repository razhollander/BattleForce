using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts
{
    public class MatchEnvironmentSpringView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Vector3 _originalScale;

        public void Initialize(Mesh mesh, Material material)
        {
            if (_meshFilter == null) _meshFilter = gameObject.GetComponent<MeshFilter>();
            if (_meshRenderer == null) _meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();
            if (_meshRenderer == null) _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (mesh != null) _meshFilter.sharedMesh = mesh;
            if (material != null) _meshRenderer.sharedMaterial = material;

            // Assuming the scale is set correctly before or during initialization
            _originalScale = transform.localScale;
        }

        public void SetOriginalScale(Vector3 scale)
        {
            _originalScale = scale;
        }

        public void PlayBounceAnimation()
        {
            transform.DOKill();
            transform.localScale = _originalScale;
            transform.DOScale(_originalScale * 1.3f, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
    }
}
