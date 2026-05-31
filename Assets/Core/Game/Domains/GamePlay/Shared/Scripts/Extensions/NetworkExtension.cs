using LiteNetLib.Utils;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Shared.Extensions
{
    public static class NetworkExtension
    {
        private static readonly float DOUBLE_PI = Mathf.PI * 2;
        
        public static void Put(this NetDataWriter writer, Vector2 vector)
        {
            writer.PutFloat16(vector.X);
            writer.PutFloat16(vector.Y);
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
            v.X = reader.GetFloat16();
            v.Y = reader.GetFloat16();
            return v;
        }

        public static void PutFloat16(this NetDataWriter writer, float value)
        {
            writer.Put(Mathf.FloatToHalf(value));
        }

        public static float GetFloat16(this NetDataReader reader)
        {
            return Mathf.HalfToFloat(reader.GetUShort());
        }

        /// <summary>
        /// Compresses a float between 0.0 and 1.0 into a single byte (8 bits).
        /// </summary>
        // public static void PutFloat8(this NetDataWriter writer, float value)
        // {
        //     value = Mathf.Clamp01(value);
        //     byte compressed = (byte)(value * 255f);
        //     writer.Put(compressed);
        // }

        /// <summary>
        /// Reads an 8-bit compressed float back into a 0.0 to 1.0 range.
        /// </summary>
        // public static float GetFloat8(this NetDataReader reader)
        // {
        //     byte compressed = reader.GetByte();
        //     return compressed / 255f;
        // }

        /// <summary>
        /// Compresses a float within a custom dynamic range into a single byte.
        /// </summary>
        public static void PutFloat8(this NetDataWriter writer, float value, float min=-64f, float max=64f)
        {
            float normalized = (value - min) / (max - min);
            normalized = Mathf.Clamp01(normalized);
            byte compressed = (byte)(normalized * 255f);
            writer.Put(compressed);
        }

        /// <summary>
        /// Reads an 8-bit compressed float and restores it over the specified custom range.
        /// </summary>
        public static float GetFloat8(this NetDataReader reader, float min=-64f, float max = 64f)
        {
            byte compressed = reader.GetByte();
            float normalized = compressed / 255f;
            return min + normalized * (max - min);
        }

        public static void PutVector2MegaQuantized(this NetDataWriter writer, Vector2 vector)
        {
            writer.PutFloat8(vector.X);
            writer.PutFloat8(vector.Y);
        }

        public static Vector2 GetVector2MegaQuantized(this NetDataReader reader)
        {
            var x = reader.GetFloat8();
            var y = reader.GetFloat8();
            return new Vector2(x, y);
        }
        
        public static void PutVector2Quantized(this NetDataWriter writer, Vector2 vector)
        {
            writer.PutFloat16(vector.X);
            writer.PutFloat16(vector.Y);
        }

        public static Vector2 GetVector2Quantized(this NetDataReader reader)
        {
            var x = reader.GetFloat16();
            var y = reader.GetFloat16();
            return new Vector2(x, y);
        }

        public static void PutVector2AsAngle16(this NetDataWriter writer, Vector2 vector)
        {
             var angle = (float)System.Math.Atan2(vector.Y, vector.X);
             var normalized = (angle + Mathf.PI) / (DOUBLE_PI);
             normalized = Mathf.Clamp01(normalized);
             var compressed = (ushort)(normalized * 65535);
             writer.Put(compressed);
        }

        public static Vector2 GetVector2FromAngle16(this NetDataReader reader)
        {
            var compressed = reader.GetUShort();
            var normalized = compressed / 65535f;
            var angle = normalized * DOUBLE_PI - Mathf.PI;
            return new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
        }
    }
}