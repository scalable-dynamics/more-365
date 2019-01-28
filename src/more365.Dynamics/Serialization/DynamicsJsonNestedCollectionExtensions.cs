using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace more365.Dynamics.Query
{
    public static class DynamicsJsonNestedCollectionExtensions
    {
        public static IEnumerable<T> CreateNestedCollectionFromJObject<T>(this IEnumerable<JObject> data)
        {
            var results = new List<T>();
            var nestedObjectFactory = GetNestedJObjectFactory<T>();
            foreach (var item in data)
            {
                var entity = nestedObjectFactory(item);
                results.Add(entity);
            }
            return results;
        }

        private static Func<JObject, T> GetNestedJObjectFactory<T>()
        {
            return (data) =>
            {
                var joinedPropertyNames = data.Properties().Where(p => p.Name.GetPropertyName().Split('@').First().Contains("."))
                                                           .Select(p => p.Name.GetNestedPropertyPrefix())
                                                           .Distinct()
                                                           .ToArray();
                var joinedPropertyValues = joinedPropertyNames.ToDictionary(j => j, j => j.CreateNestedJObject(data.Properties()));
                return (T)CreateNestedObject(typeof(T), data, joinedPropertyValues);
            };
        }

        private static string GetPropertyName(this string name)
        {
            return name.Replace("_x002e_", ".");
        }

        private static string GetNestedPropertyPrefix(this string path)
        {
            var propertyName = path.GetPropertyName();
            return propertyName.Substring(0, propertyName.IndexOf("."));
        }

        private static string GetNestedPropertyName(this string path)
        {
            var propertyName = path.GetPropertyName();
            return propertyName.Substring(propertyName.IndexOf(".") + 1);
        }

        private static bool IsNestedPropertyName(this string path, string prefix)
        {
            var propertyName = path.GetPropertyName();
            return propertyName.StartsWith(prefix + ".");
        }

        private static object CreateNestedObject(Type type, JObject data, Dictionary<string, JObject> joinedPropertyValues)
        {
            var instance = data.ToObject(type);
            foreach (var join in joinedPropertyValues.Keys)
            {
                var property = type.GetProperty(join);
                if (property != null && property.CanWrite && property.PropertyType.IsClass)
                {
                    var joinedData = joinedPropertyValues[property.Name];
                    var entity = CreateNestedObject(property.PropertyType, joinedData, joinedPropertyValues);
                    property.SetValue(instance, entity);
                }
            }
            return instance;
        }

        private static JObject CreateNestedJObject(this string prefix, IEnumerable<JProperty> properties)
        {
            var obj = new JObject();
            var names = new List<string>();
            foreach (var jProp in properties)
            {
                if (jProp.Name.IsNestedPropertyName(prefix))
                {
                    var property = jProp.Name.GetNestedPropertyName();
                    if (!names.Contains(property))
                    {
                        names.Add(property);
                        obj[property] = jProp.Value;
                    }
                }
            }
            return obj;
        }
    }
}
