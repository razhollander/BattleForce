using System.Numerics;
using LiteNetLib.Utils;
using Core.Game.Domains.GamePlay.Shared.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct EnvironmentWallStateS2C
    {
        public ushort Id;
        public Vector2[] Points;
        
        public EnvironmentWallStateS2C(ushort id, Vector2[] points)
        {
            Id = id;
            Points = points;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)Points.Length);

            foreach (var point in Points)
            {
                writer.Put(point);
            }
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            var pointsAmount = reader.GetByte();
            Points = new Vector2[pointsAmount];
            if (pointsAmount > 0)
            {
                for (int i = 0; i < pointsAmount; i++)
                {
                    Points[i] = reader.GetVector2();
                }
            }
        }
    }
}