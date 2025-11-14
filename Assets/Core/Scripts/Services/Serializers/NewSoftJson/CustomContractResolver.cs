using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CoreDomain.Scripts.Services.Serializers.NewSoftJson
{
    public class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo memberInfo, MemberSerialization memberSerialization)
        {
            return CreateAbilityReadPrivateSetterProperty(memberInfo, memberSerialization);
        }

        private JsonProperty CreateAbilityReadPrivateSetterProperty(MemberInfo memberInfo, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(memberInfo, memberSerialization);

            if (jsonProperty.Writable)
            {
                return jsonProperty;
            }

            var property = memberInfo as PropertyInfo;

            if (property == null)
            {
                return jsonProperty;
            }

            var hasPrivateSetter = property.GetSetMethod(true) != null;
            jsonProperty.Writable = hasPrivateSetter;

            return jsonProperty;
        }
    }
}