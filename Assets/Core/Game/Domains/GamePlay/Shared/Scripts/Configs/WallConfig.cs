using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    public class WallConfig
    {
        public ushort Id;
        public Vector2[] Points;

        public WallConfig(ushort id, Vector2[] points)
        {
            Id = id;
            Points = points;
        }
    }
}