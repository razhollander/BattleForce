using CoreDomain.Scripts.Services.Serializers.Serializer;
using Newtonsoft.Json;

namespace CoreDomain.Scripts.Services.Serializers.NewSoftJson
{
    public class NewSoftJsonSerializer : ISerializer<string>
    {
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new CustomContractResolver()
        };

        public string Serialize<TIn>(TIn obj)
        {
            return JsonConvert.SerializeObject(obj, _serializerSettings);
        }

        public TOut Deserialize<TOut>(string obj)
        {
            return JsonConvert.DeserializeObject<TOut>(obj, _serializerSettings);
        }

        public void Populate(string value, object target)
        {
            JsonConvert.PopulateObject(value, target, _serializerSettings);
        }
    }
}