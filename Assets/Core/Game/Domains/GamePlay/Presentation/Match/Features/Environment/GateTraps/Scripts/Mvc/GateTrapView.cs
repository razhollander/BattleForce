using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.GateTraps.Scripts.Mvc
{
    public class GateTrapView : MonoBehaviour
    {
        private static readonly int COLOR_PROPERTY_ID = Shader.PropertyToID("_Color");
        private static readonly int BASE_COLOR_PROPERTY_ID = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;

        private MaterialPropertyBlock _materialPropertyBlock;

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
        }

        /// <summary>
        /// The gate trap's wall greys out while it cools down, so the property block is created on the first tint.
        /// </summary>
        public void SetColor(Color color)
        {
            _materialPropertyBlock ??= new MaterialPropertyBlock();

            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor(COLOR_PROPERTY_ID, color);
            _materialPropertyBlock.SetColor(BASE_COLOR_PROPERTY_ID, color);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}
