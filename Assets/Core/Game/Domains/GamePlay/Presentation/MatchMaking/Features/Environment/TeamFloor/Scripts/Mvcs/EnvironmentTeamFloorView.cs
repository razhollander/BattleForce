using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs
{
    public class EnvironmentTeamFloorView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        
        public void Setup(Mesh mesh, Material material)
        {
            _meshFilter.sharedMesh = mesh;
            _meshRenderer.sharedMaterial = material;
        }
    }
}
