using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace more365.Dynamics.Query
{
    public static class QueryExtensions
    {
        public static IXrmQueryExpression CreateQuery(this IDynamicsClient dynamicsClient, string entityLogicalName)
        {
            return new XrmQuery(entityLogicalName);
        }

        public static string ToFetchUrl(this IXrmQueryExpression query, string entitySetName, int? maxRecordCount = null)
        {
            return ((IXrmQuery)query).ToFetchUrl(entitySetName, maxRecordCount);
        }

        public static string ToFetchXml(this IXrmQueryExpression query, int? maxRecordCount = null, bool stripWhitespace = true)
        {
            return ((IXrmQuery)query).ToFetchXml(maxRecordCount, stripWhitespace);
        }

        public static Task<IEnumerable<T>> ExecuteQuery<T>(this IDynamicsClient dynamicsClient, IXrmQueryExpression query, string entitySetName, int? maxRecordCount = null)
        {
            var url = query.ToFetchUrl(entitySetName, maxRecordCount);
            return dynamicsClient.ExecuteQuery<T>(url);
        }

        public static async Task<T> Get<T>(this IDynamicsClient dynamicsClient, IXrmQueryExpression query, string entitySetName)
        {
            var url = query.ToFetchUrl(entitySetName, 1);
            var results = await dynamicsClient.ExecuteQuery<T>(url);
            return results.FirstOrDefault();
        }

        public static Task<T> Get<T>(this IDynamicsClient dynamicsClient, Guid id, params string[] columns)
        {
            return dynamicsClient.Get<T>(typeof(T).GetEntitySetName(), id, columns);
        }
    }
}
