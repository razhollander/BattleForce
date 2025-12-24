using UnityEngine;

public class EnvironmentWallView : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;

    public void SetMesh(Mesh mesh)
    {
        _meshFilter.sharedMesh = mesh;
    }
}
