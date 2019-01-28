using more365.Dynamics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace more365
{
    public interface IDynamicsClient
    {
        Task<IEnumerable<T>> ExecuteBatch<T>(params BatchRequest[] requests);

        Task<IEnumerable<T>> ExecuteQuery<T>(string url);

        Task<T> ExecuteSingle<T>(string url);

        Task<T> Get<T>(string entitySetName, Guid id, params string[] columns);

        Task<Guid> Save(string entitySetName, object data, Guid? id = null);
    }
}