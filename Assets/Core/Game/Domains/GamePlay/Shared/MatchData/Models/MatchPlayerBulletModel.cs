using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.MatchData.Models
{
    public class MatchPlayerBulletModel
    {
        public int Id;
        public int BelongToPlayerId;
        public Vector2 Position;

        public MatchPlayerBulletModel(int id, int belongToPlayerId, Vector2 position)
        {
            Id = id;
            BelongToPlayerId = belongToPlayerId;
            Position = position;
        }
    }
}