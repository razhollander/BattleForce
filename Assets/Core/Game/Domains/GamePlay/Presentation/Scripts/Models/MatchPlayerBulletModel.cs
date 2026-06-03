using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchPlayerBulletModel
    {
        public ushort Id;
        public ushort BelongToPlayerId;
        public Vector2 Position;
        public Vector2 InitialPosition;
        public Vector2 Velocity;
        public float Radius;
        public int SpawnTick;

        public MatchPlayerBulletModel(ushort id, ushort belongToPlayerId, Vector2 initialPosition, Vector2 velocity, float radius, int spawnTick)
        {
            Id = id;
            BelongToPlayerId = belongToPlayerId;
            Position = initialPosition;
            InitialPosition = initialPosition;
            Velocity = velocity;
            Radius = radius;
            SpawnTick = spawnTick;
        }
    }
}