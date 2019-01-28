using Newtonsoft.Json;

namespace more365.Dynamics.Serialization
{
    internal static class DynamicsSerializer
    {
        private static JsonSerializerSettings _deserializingJsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DynamicsJsonContractResolver(isDeserializing: true)
        };

        private static JsonSerializerSettings _serializingJsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DynamicsJsonContractResolver(isDeserializing: false)
        };

        public static T DeserializeObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _deserializingJsonSerializerSettings);
        }

        public static string SerializeObject(this object obj)
        {
            return JsonConvert.SerializeObject(obj, _serializingJsonSerializerSettings);
        }
    }
}