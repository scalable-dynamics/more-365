using more365.Dynamics;
using more365.Graph;
using more365.SharePoint;
using System;

namespace more365
{
    public class More365ClientFactory : IMore365ClientFactory
    {
        protected readonly More365Configuration _config;
        protected readonly IAuthenticatedHttpClientFactory _authenticatedHttpClientFactory;

        public More365ClientFactory(IAuthenticatedHttpClientFactory authenticatedHttpClientFactory, More365Configuration config)
        {
            _config = config;
            _authenticatedHttpClientFactory = authenticatedHttpClientFactory;
        }

        public IDynamicsClient CreateDynamicsClient(Guid? impersonateAzureADObjectId = null)
        {
            var httpClient = _authenticatedHttpClientFactory.CreateAuthenticatedHttpClient(_config.DynamicsUrl.ToString(), impersonateAzureADObjectId);
            httpClient.BaseAddress = _config.DynamicsUrl;
            httpClient.Timeout = new TimeSpan(0, 2, 0);
            if (impersonateAzureADObjectId.HasValue)
            {
                httpClient.DefaultRequestHeaders.Remove("CallerObjectId");
                httpClient.DefaultRequestHeaders.Add("CallerObjectId", impersonateAzureADObjectId.ToString());
            }
            return new DynamicsClient(httpClient);
        }

        public IGraphClient CreateGraphClient()
        {
            var httpClient = _authenticatedHttpClientFactory.CreateAuthenticatedHttpClient(GraphClient.MicrosoftGraphUrl.ToString());
            httpClient.BaseAddress = GraphClient.MicrosoftGraphUrl;
            httpClient.Timeout = new TimeSpan(0, 2, 0);
            return new GraphClient(httpClient);
        }

        public ISharePointClient CreateSharePointClient()
        {
            var httpClient = _authenticatedHttpClientFactory.CreateAuthenticatedHttpClient(_config.SharePointUrl.ToString());
            httpClient.BaseAddress = _config.SharePointUrl;
            httpClient.Timeout = new TimeSpan(0, 2, 0);
            return new SharePointClient(httpClient);
        }
    }
}