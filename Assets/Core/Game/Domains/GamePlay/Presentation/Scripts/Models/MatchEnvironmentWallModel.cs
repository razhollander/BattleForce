using System.Numerics;
using Vector3 = UnityEngine.Vector3;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentWallModel
    {
        public ushort Id;
        public Vector2[] Points;
        public Vector2 LocalPosition;

        public MatchEnvironmentWallModel(ushort id, Vector2[] points, Vector2 localPosition)
        {
            Id = id;
            Points = points;
            LocalPosition = localPosition;
        }
    }
}