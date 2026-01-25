using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.MatchData.Models
{
    public class MatchEnvironmentLavaWallModel
    {
        public ushort Id;
        public Vector2[] Points;

        public MatchEnvironmentLavaWallModel(ushort id, Vector2[] points)
        {
            Id = id;
            Points = points;
        }
    }
}