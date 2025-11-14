namespace CoreDomain.Scripts.Services.Serializers.Serializer
{
    public interface ISerializerService
    {
        string SerializeJson<T>(T obj);
        T DeserializeJson<T>(string json);
    }
}