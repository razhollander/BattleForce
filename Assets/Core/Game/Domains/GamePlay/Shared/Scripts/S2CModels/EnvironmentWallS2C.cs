using System;
using System.Numerics;
using Box2D.NetStandard.Common;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class EnvironmentWallS2C : IEquatable<ushort>
    {
        public ushort Id;

        public Vector2[] Points { get; }
        public int PointsCount { get; private set; }

        public void SetPoints(Vector2[] points)
        {
            PointsCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                Points[i] = points[i];
            }
        }
        
        public EnvironmentTransformS2C Transform = new();

        public EnvironmentWallS2C()
        {
            Points = new Vector2[Settings.MaxPolygonVertices];
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}