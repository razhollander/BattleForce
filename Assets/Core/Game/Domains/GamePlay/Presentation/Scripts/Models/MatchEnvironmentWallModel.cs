using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentWallModel
    {
        public ushort Id;
        public Vector2[] Points;

        public MatchEnvironmentWallModel(ushort id, Vector2[] points)
        {
            Id = id;
            Points = points;
        }
    }
}