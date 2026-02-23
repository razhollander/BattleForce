using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    public class WallConfig
    {
        public ushort Id;
        public Vector2[] Points;

        [NonSerialized]
        public float WorldRotationAngle;
        [NonSerialized]
        public Vector2 WorldPosition;
        
        public WallConfig(ushort id, Vector2[] points)
        {
            Id = id;
            Points = points;
        }
    }
}