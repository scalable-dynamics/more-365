using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace more365.Dynamics.Query
{
    internal class NestedJsonCollectionReader<T>
    {
        private IEnumerable<JsonElement> _data;

        public IEnumerable<T> ReadData() => CreateNestedCollectionFromJObject(_data);

        public NestedJsonCollectionReader(IEnumerable<JsonElement> data)
        {
            _data = data;
        }

        private static IEnumerable<T> CreateNestedCollectionFromJObject(IEnumerable<JsonElement> data)
        {
            var results = new List<T>();
            var nestedObjectFactory = GetNestedJObjectFactory();
            foreach (var item in data)
            {
                var entity = nestedObjectFactory(item);
                results.Add(entity);
            }
            return results;
        }

        private static Func<JsonElement, T> GetNestedJObjectFactory()
        {
            return (data) =>
            {
                var joinedPropertyNames = data.EnumerateObject()
                                              .Where(p => formatPropertyName(p.Name).Split('@').First().Contains("."))
                                              .Select(p => getNestedPropertyPrefix(p.Name))
                                              .Distinct()
                                              .ToArray();
                var joinedPropertyValues = joinedPropertyNames.ToDictionary(j => j, j => CreateNestedObjects(j, data.EnumerateObject()));
                return (T)CreateNestedObject(typeof(T), CreateNestedObjects("",data.EnumerateObject()), joinedPropertyValues);
            };
        }

        private static string formatPropertyName(string name)
        {
            return name.Replace("_x002e_", ".");
        }

        private static string getNestedPropertyPrefix(string path)
        {
            var propertyName = formatPropertyName(path);
            return propertyName.Substring(0, propertyName.IndexOf("."));
        }

        private static string getNestedPropertyName(string path)
        {
            var propertyName = formatPropertyName(path);
            return propertyName.Substring(propertyName.IndexOf(".") + 1);
        }

        private static bool isNestedPropertyName(string path, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return true;
            }
            else
            {
                var propertyName = formatPropertyName(path);
                return propertyName.StartsWith(prefix + ".");
            }
        }

        private static object CreateNestedObject(Type type, Dictionary<string, string> data, Dictionary<string, Dictionary<string, string>> joinedPropertyValues)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var jsonProperty in data)
            {
                var property = type.GetProperty(jsonProperty.Key);
                if (property != null && property.CanWrite && property.PropertyType.IsClass)
                {
                    property.SetValue(instance, jsonProperty.Value);
                }
            }
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

        private static Dictionary<string, string> CreateNestedObjects(string prefix, IEnumerable<JsonProperty> properties)
        {
            var result = new Dictionary<string, string>();
            var names = new List<string>();
            foreach (var jProp in properties)
            {
                if (isNestedPropertyName(jProp.Name, prefix))
                {
                    var property = getNestedPropertyName(jProp.Name);
                    if (!names.Contains(property))
                    {
                        names.Add(property);
                        result[property] = jProp.Value.GetString();
                    }
                }
            }
            return result;
        }
    }
}
