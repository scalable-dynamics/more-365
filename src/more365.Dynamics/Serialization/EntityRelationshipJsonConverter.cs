using more365.Dynamics.Query;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace more365.Dynamics.Serialization
{
    public class EntityRelationshipJsonConverter : JsonConverter
    {
        private readonly PropertyInfo _id;
        private readonly string _entitySetName;

        public EntityRelationshipJsonConverter(PropertyInfo property)
        {
            if (typeof(string).IsAssignableFrom(property.PropertyType) || typeof(Guid).IsAssignableFrom(property.PropertyType))
            {
                _entitySetName = property.GetEntitySetName();
            }
            else
            {
                _id = property.PropertyType.GetProperty(property.PropertyType.Name + "Id") ?? property.PropertyType.GetProperty("Id");
                _entitySetName = property.PropertyType.GetEntitySetName();
            }
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type type) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null && value is string id)
            {
                var obj = Activator.CreateInstance(objectType);
                _id.SetValue(obj, new Guid(id));
                return obj;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                if (_id == null)
                {
                    writer.WriteValue($"/{_entitySetName}({value})");
                }
                else
                {
                    var id = _id.GetValue(value);
                    if (id != null)
                    {
                        writer.WriteValue($"/{_entitySetName}({id})");
                    }
                    else
                    {
                        writer.WriteNull();
                    }
                }
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}