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
            writer.PutFloat16(color.r);
            writer.PutFloat16(color.g);
            writer.PutFloat16(color.b);
        }
        
        public static Color GetColor(this NetDataReader reader)
        {
            return new Color(reader.GetFloat16(), reader.GetFloat16(), reader.GetFloat16());
        }
        
        public static Vector2 GetVector2(this NetDataReader reader)
        {
            Vector2 v;
            v.X = reader.GetFloat();
            v.Y = reader.GetFloat();
            return v;
        }

        public static void PutFloat16(this NetDataWriter writer, float value)
        {
            writer.Put((ushort)Mathf.FloatToHalf(value));
        }

        public static float GetFloat16(this NetDataReader reader)
        {
            return Mathf.HalfToFloat(reader.GetUShort());
        }

        public static void PutVector2Quantized(this NetDataWriter writer, Vector2 vector)
        {
            writer.PutFloat16(vector.X);
            writer.PutFloat16(vector.Y);
        }

        public static Vector2 GetVector2Quantized(this NetDataReader reader)
        {
            float x = reader.GetFloat16();
            float y = reader.GetFloat16();
            return new Vector2(x, y);
        }

        public static void PutVector2AsAngle16(this NetDataWriter writer, Vector2 vector)
        {
             float angle = (float)System.Math.Atan2(vector.Y, vector.X);
             float normalized = (angle + Mathf.PI) / (Mathf.PI * 2);
             normalized = Mathf.Clamp01(normalized);
             ushort compressed = (ushort)(normalized * 65535);
             writer.Put(compressed);
        }

        public static Vector2 GetVector2FromAngle16(this NetDataReader reader)
        {
            ushort compressed = reader.GetUShort();
            float normalized = compressed / 65535f;
            float angle = normalized * Mathf.PI * 2 - Mathf.PI;
            return new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
        }
    }
}