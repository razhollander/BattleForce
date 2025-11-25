using System.Numerics;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Extensions
{
    public static class NetworkExtension
    {
        public static void Put(this NetDataWriter writer, Vector2 vector)
        {
            writer.Put(vector.X);
            writer.Put(vector.Y);
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