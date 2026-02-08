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
