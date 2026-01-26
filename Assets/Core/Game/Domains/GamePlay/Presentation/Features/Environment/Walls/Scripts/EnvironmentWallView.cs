using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts
{
    public class EnvironmentWallView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
        }

        public void SetColor(Color color)
        {
            _meshRenderer.material.color = color;
        }

        public void AnimateBounce()
        {
            transform.DOScale(1.2f, 0.15f).SetLoops(2, LoopType.Yoyo);
        }
    }
}
