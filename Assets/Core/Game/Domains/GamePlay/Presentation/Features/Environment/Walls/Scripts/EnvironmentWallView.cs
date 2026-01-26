using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts
{
    public class EnvironmentWallView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
        }
    }
}
