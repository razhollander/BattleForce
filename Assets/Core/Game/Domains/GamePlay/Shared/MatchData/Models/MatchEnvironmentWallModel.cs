using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.MatchData.Models
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