using System.Text;
using Newtonsoft.Json;
using Sirenix.Serialization;

namespace Core.Scripts.Extensions
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string ToJson(this object obj)
        {
            var bytes = SerializationUtility.SerializeValue(obj, DataFormat.JSON);
            return Encoding.UTF8.GetString(bytes);
        }
        
        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        public static string Format(this string formatString, params object[] args)
        {
            return string.Format(formatString, args);
        }
    }
}
