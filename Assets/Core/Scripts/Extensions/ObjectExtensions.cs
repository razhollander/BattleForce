using System.Text;
using Newtonsoft.Json;
using Sirenix.Serialization;

namespace Core.Scripts.Extensions
{
    public static class ObjectExtensions
    {
        public static T DeepClone<T>(this T self)
        {
            // Serialize with TypeNameHandling if you have derived classes/polymorphism
            var settings = new JsonSerializerSettings 
            { 
                ObjectCreationHandling = ObjectCreationHandling.Replace 
            };

            var json = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        
        public static string ToJson(this object obj)
        {
            var bytes = SerializationUtility.SerializeValue(obj, DataFormat.JSON);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}