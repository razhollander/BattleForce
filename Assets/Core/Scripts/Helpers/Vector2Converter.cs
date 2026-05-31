using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Core.Scripts.Helpers
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            // Only save the x and y values
            JObject obj = new JObject
            {
                { "x", value.x },
                { "y", value.y }
            };
            obj.WriteTo(writer);
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
        
            float x = obj["x"] != null ? (float)obj["x"] : 0f;
            float y = obj["y"] != null ? (float)obj["y"] : 0f;
        
            return new Vector2(x, y);
        }
    }
}