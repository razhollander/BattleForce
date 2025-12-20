using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.MatchData.Models
{
    public class MatchPlayerBulletModel
    {
        public int Id;
        public int BelongToPlayerId;
        public Vector2 Position;
        public float Radius;

        public MatchPlayerBulletModel(int id, int belongToPlayerId, Vector2 position, float radius)
        {
            Id = id;
            BelongToPlayerId = belongToPlayerId;
            Position = position;
            Radius = radius;
        }
    }
}