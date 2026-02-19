using UnityEngine;
using UnityEngine.ProBuilder;

namespace Core.Game.Domains.GamePlay.Shared.LevelEnvironment.Scripts
{
    public class WallGenerator : MonoBehaviour
    {
        [SerializeField] private ProBuilderMesh _proBuilderMesh;

        public System.Numerics.Vector2[] GetPoints()
        {
            return ProBuilderVertexUtils.GetVerticesCCW_XY(_proBuilderMesh);
        }
    }
}
