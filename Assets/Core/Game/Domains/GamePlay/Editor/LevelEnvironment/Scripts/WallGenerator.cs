using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Core.Game.Domains.GamePlay.Editor.LevelEnvironment.Scripts
{
    public class WallGenerator : MonoBehaviour
    {
        [SerializeField] private ProBuilderMesh _proBuilderMesh;

        public List<System.Numerics.Vector2> GetPoints()
        {
            var points = new List<System.Numerics.Vector2>();

            foreach (var vertex in _proBuilderMesh.positions)
            {
                var point = new System.Numerics.Vector2(vertex.x, vertex.z);
                points.Add(point);
            }

            return points;
        }
    }
}
