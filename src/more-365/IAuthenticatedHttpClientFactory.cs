using System;
using System.Net.Http;

namespace more365
{
    public interface IAuthenticatedHttpClientFactory
    {
        HttpClient CreateAuthenticatedHttpClient(string resource, Guid? uniqueId = null);
    }
}