using System;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Helpers.JsonConverters
{
    public class TalentCooldownConfigConverter : JsonConverter
    {
        // We only need custom logic for reading. 
        // Setting this to false lets Newtonsoft handle serialization (writing) normally.
        public override bool CanWrite => false; 

        // public override bool CanConvert(Type objectType)
        // {
        //     // This ensures the converter applies to the base class and its children
        //     return typeof(BaseTalentCooldownConfig).IsAssignableFrom(objectType);
        // }
        
        public override bool CanConvert(Type objectType)
        {
            // CRITICAL FIX: Only return true for the exact base class.
            // If we use IsAssignableFrom, the serializer will trigger this converter AGAIN 
            // when trying to populate the derived class, causing a Stack Overflow.
            return objectType == typeof(BaseTalentCooldownConfig);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Handle null values safely
            if (reader.TokenType == JsonToken.Null) return null;

            // Load the JSON into a JObject so we can inspect it before deserializing
            JObject jsonObject = JObject.Load(reader);

            // Look for the "CooldownType" property in the JSON
            if (!jsonObject.TryGetValue("CooldownType", StringComparison.OrdinalIgnoreCase, out JToken cooldownTypeToken))
            {
                throw new JsonSerializationException("Cannot deserialize talent config: Missing 'CooldownType' discriminator.");
            }

            // Parse the token into your enum
            var cooldownType = cooldownTypeToken.ToObject<TalentCooldownType>();

            // Instantiate the correct derived class based on the enum
            BaseTalentCooldownConfig config = cooldownType switch
            {
                TalentCooldownType.AlwaysActive => new TalentAlwaysActiveCooldownConfig(),
                TalentCooldownType.Normal => new TalentNormalCooldownConfig(),
                TalentCooldownType.Stocks => new TalentStocksCooldownConfig(),
                _ => throw new JsonSerializationException($"Unknown CooldownType: {cooldownType}")
            };

            // Populate the freshly instantiated object with the rest of the JSON data
            serializer.Populate(jsonObject.CreateReader(), config);

            return config;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // This will not be called because CanWrite is false.
            throw new NotImplementedException();
        }
    }
}