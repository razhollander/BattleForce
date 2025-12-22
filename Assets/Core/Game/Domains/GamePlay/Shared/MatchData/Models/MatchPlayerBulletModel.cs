using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.MatchData.Models
{
    public class MatchPlayerBulletModel
    {
        public ushort Id;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public float Radius;

        public MatchPlayerBulletModel(ushort id, ushort belongToPlayerId, Vector2 position, float radius)
        {
            Id = id;
            BelongToPlayerId = belongToPlayerId;
            Position = position;
            Radius = radius;
        }
    }
}