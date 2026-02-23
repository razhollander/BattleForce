using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchEnvironmentLavaWallModel
    {
        public ushort Id;
        public Vector2[] Points;
        public Vector2 LocalPosition;

        public MatchEnvironmentLavaWallModel(ushort id, Vector2[] points, Vector2 localPosition)
        {
            Id = id;
            Points = points;
            LocalPosition = localPosition;
        }
    }
}