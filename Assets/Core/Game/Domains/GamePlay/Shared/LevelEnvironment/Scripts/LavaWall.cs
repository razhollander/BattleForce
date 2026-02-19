using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.LevelEnvironment.Scripts
{
    [RequireComponent(typeof(PolygonPath2D))]
    public class LavaWall : MonoBehaviour
    {
        public List<Vector2> GetPoints()
        {
            var poly = GetComponent<PolygonPath2D>();
            return poly.GetPointsRelativeToObject();
        }
    }
}
