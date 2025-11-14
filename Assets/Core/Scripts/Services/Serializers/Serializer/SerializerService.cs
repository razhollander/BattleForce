using CoreDomain.Scripts.Services.Serializers.NewSoftJson;

namespace CoreDomain.Scripts.Services.Serializers.Serializer
{
    public class SerializerService : ISerializerService
    {
        private readonly ISerializer<string> _jsonSerializer = new NewSoftJsonSerializer();

        public string SerializeJson<T>(T obj)
        {
            return _jsonSerializer.Serialize(obj);
        }

        public T DeserializeJson<T>(string json)
        {
            return _jsonSerializer.Deserialize<T>(json);
        }
    }
}