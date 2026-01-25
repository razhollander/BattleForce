using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts
{
    public class EnvironmentLavaWallView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
        }
    }
}
