using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace more365.Dynamics.Query
{
    public static class EntityQueryExtensions
    {
        public static IEntityQueryExpression<T> CreateQuery<T>(this IDynamicsClient dynamicsClient) where T : class, new()
        {
            return new EntityQuery<T>();
        }

        public static string ToFetchUrl<T>(this IEntityQueryExpression<T> query, int? maxRecordCount = null) where T : class, new()
        {
            var entitySetName = typeof(T).GetEntitySetName();
            return ((IXrmQuery)query).ToFetchUrl(entitySetName, maxRecordCount);
        }

        public static string ToFetchXml<T>(this IEntityQueryExpression<T> query, int? maxRecordCount = null, bool stripWhitespace = false) where T : class, new()
        {
            return ((IXrmQuery)query).ToFetchXml(maxRecordCount, stripWhitespace);
        }

        public static async Task<IEnumerable<T>> ExecuteQuery<T>(this IDynamicsClient dynamicsClient, IEntityQueryExpression<T> query, int? maxRecordCount = null) where T : class, new()
        {
            var url = query.ToFetchUrl<T>(maxRecordCount);
            var data = await dynamicsClient.ExecuteQuery<JObject>(url);
            return data.CreateNestedCollectionFromJObject<T>();
        }

        public static async Task<T> Get<T>(this IDynamicsClient dynamicsClient, IEntityQueryExpression<T> query) where T : class, new()
        {
            var url = query.ToFetchUrl<T>(1);
            var results = await dynamicsClient.ExecuteQuery<T>(url);
            return results.FirstOrDefault();
        }

        internal static string GetEntityLogicalName(this Type type, string entityLogicalName = null)
        {
            if (!string.IsNullOrEmpty(entityLogicalName))
            {
                return entityLogicalName;
            }
            else
            {
                var entityNameAttribute = (EntityNameAttribute)type.GetCustomAttributes(typeof(EntityNameAttribute), true).FirstOrDefault();
                if (entityNameAttribute != null)
                {
                    return entityNameAttribute.EntityLogicalName;
                }
                else
                {
                    return type.Name.ToLower();
                }
            }
        }

        internal static string GetEntitySetName(this Type type, string entitySetName = null)
        {
            if (!string.IsNullOrEmpty(entitySetName))
            {
                return entitySetName;
            }
            else
            {
                var entityNameAttribute = (EntityNameAttribute)type.GetCustomAttributes(typeof(EntityNameAttribute), true).FirstOrDefault();
                if (entityNameAttribute != null)
                {
                    return entityNameAttribute.EntitySetName;
                }
                else
                {
                    return type.Name.ToLower();
                }
            }
        }

        internal static string GetEntitySetName(this PropertyInfo property)
        {
            var entityNameAttribute = (EntityNameAttribute)property.GetCustomAttributes(typeof(EntityNameAttribute), true).FirstOrDefault();
            if (entityNameAttribute != null)
            {
                return entityNameAttribute.EntitySetName;
            }
            else
            {
                return property.PropertyType.GetEntitySetName();
            }
        }

        internal static string GetPropertyName(this MemberInfo property, bool includeBinding = false)
        {
            var propertyName = property.Name.ToLower();

            var jsonNameAttribute = (JsonPropertyAttribute)property.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault();
            if (jsonNameAttribute != null)
            {
                propertyName = jsonNameAttribute.PropertyName;
            }
            else
            {
                var propertyNameAttribute = (AttributeNameAttribute)property.GetCustomAttributes(typeof(AttributeNameAttribute), true).FirstOrDefault();
                if (propertyNameAttribute != null)
                {
                    propertyName = propertyNameAttribute.PropertyName;
                }
            }

            if (!includeBinding && !string.IsNullOrEmpty(propertyName))
            {
                if (propertyName.Contains("@"))
                {
                    propertyName = propertyName.Split('@').First();
                }
                else if (propertyName.StartsWith("_"))
                {
                    propertyName = propertyName.Substring(1, propertyName.IndexOf("_value"));
                }
            }

            return propertyName;
        }
    }
}