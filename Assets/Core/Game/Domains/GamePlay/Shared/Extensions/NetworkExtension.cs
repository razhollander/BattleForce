using LiteNetLib.Utils;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Shared.Extensions
{
    public static class NetworkExtension
    {
        public static void Put(this NetDataWriter writer, Vector2 vector)
        {
            writer.Put(vector.X);
            writer.Put(vector.Y);
        }
        
        public static void Put(this NetDataWriter writer, Color color)
        {
            writer.Put(color.r);
            writer.Put(color.g);
            writer.Put(color.b);
        }
        
        public static Color GetColor(this NetDataReader reader)
        {
            return new Color(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        
        public static Vector2 GetVector2(this NetDataReader reader)
        {
            Vector2 v;
            v.X = reader.GetFloat();
            v.Y = reader.GetFloat();
            return v;
        }
    }
}