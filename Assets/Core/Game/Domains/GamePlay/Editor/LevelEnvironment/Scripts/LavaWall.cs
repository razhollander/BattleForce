using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Editor.LevelEnvironment.Scripts
{
    // A marker component for Lava Walls in the Editor scene
    [RequireComponent(typeof(PolygonPath2D))]
    public class LavaWall : MonoBehaviour
    {
        public Vector2[] GetPoints()
        {
            var poly = GetComponent<PolygonPath2D>();
            return poly.GetPointsRelativeToObject();
        }
    }
}
