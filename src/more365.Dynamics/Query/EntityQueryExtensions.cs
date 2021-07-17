using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

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
            var data = await dynamicsClient.ExecuteQuery<JsonElement>(url);
            var reader = new NestedJsonCollectionReader<T>(data);
            return reader.ReadData();
        }

        public static async Task<T> Get<T>(this IDynamicsClient dynamicsClient, IEntityQueryExpression<T> query) where T : class, new()
        {
            var url = query.ToFetchUrl<T>(1);
            var results = await dynamicsClient.ExecuteQuery<T>(url);
            return results.FirstOrDefault();
        }

        internal static string GetEntityLogicalName(this Type type, string entityLogicalName = null) => GetTableName(type) ?? entityLogicalName;

        internal static string GetEntitySetName(this Type type, string entitySetName = null) => GetTableName(type, true) ?? entitySetName;

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

        internal static string GetTableName(this Type type, bool useSetName = false)
        {
            var tableName = type.Name.ToLower();
            var tableAttribute = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            if (tableAttribute != null)
            {
                tableName = tableAttribute.Name;
                if (useSetName)
                {
                    tableName += "s";
                }
            }
            else
            {
                var entityNameAttribute = (EntityNameAttribute)type.GetCustomAttributes(typeof(EntityNameAttribute), true).FirstOrDefault();
                if (entityNameAttribute != null)
                {
                    if (useSetName)
                    {
                        tableName = entityNameAttribute.EntitySetName;
                    }
                    else
                    {
                        tableName = entityNameAttribute.EntityLogicalName;
                    }
                }
                else if (useSetName)
                {
                    tableName += "s";
                }
            }

            return tableName;
        }

        internal static string GetPropertyName(this MemberInfo property, bool includeBinding = false)
        {
            var propertyName = property.Name.ToLower();
            var ColumnAttribute = (ColumnAttribute)property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();
            if (ColumnAttribute != null)
            {
                propertyName = ColumnAttribute.Name;
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