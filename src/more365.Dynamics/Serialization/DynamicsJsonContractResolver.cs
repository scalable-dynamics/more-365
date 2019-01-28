using more365.Dynamics.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace more365.Dynamics.Serialization
{
    public class DynamicsJsonContractResolver : DefaultContractResolver
    {
        private const string ODataRelatedValue = "odata.bind";
        private const string ODataFormattedValue = "odata.community.display.v1.formattedvalue";

        public bool IsDeserializing { get; set; }

        public DynamicsJsonContractResolver(bool isDeserializing = true)
        {
            IsDeserializing = isDeserializing;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            var propertyBinding = member.GetPropertyName(true);
            if (propertyBinding.EndsWith("@" + ODataRelatedValue))
            {
                jsonProperty.PropertyName = IsDeserializing ? propertyBinding : $"_{propertyBinding.Split('@')[0]}_value";
                jsonProperty.Converter = new EntityRelationshipJsonConverter(member as PropertyInfo);
            }
            else if (propertyBinding.EndsWith("@" + ODataFormattedValue))
            {
                jsonProperty.Ignored = !IsDeserializing;
            }
            else if (propertyBinding != jsonProperty.PropertyName)
            {
                jsonProperty.PropertyName = propertyBinding;
            }
            return jsonProperty;
        }
    }
}
