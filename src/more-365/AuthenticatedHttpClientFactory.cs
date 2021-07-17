using more365.AzureAD;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace more365
{
    public class AuthenticatedHttpClientFactory : IAuthenticatedHttpClientFactory
    {
        protected readonly More365Configuration _config;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IAuthenticationClient _authenticationClient;

        public AuthenticatedHttpClientFactory(IHttpClientFactory httpClientFactory, More365Configuration config)
        {
            if (string.IsNullOrWhiteSpace(config.AzureADAppCertificateKey) && string.IsNullOrWhiteSpace(config.AzureADAppClientSecretKey))
            {
                throw new More365Exception("The application encountered an error while retrieving configuration for more365", "AzureADAppCertificateKey or AzureADAppClientSecretKey is required");
            }
            _config = config;
            _httpClientFactory = httpClientFactory;
            if(config.AzureKeyVaultUrl != default)
            {
                if (!string.IsNullOrWhiteSpace(config.AzureADAppCertificateKey))
                {
                    _authenticationClient = new SecretAuthenticationClient(config.AzureADTenantId, config.AzureADApplicationId, config.AzureKeyVaultUrl, config.AzureADAppCertificateKey);
                }
                else
                {
                    _authenticationClient = new SecretAuthenticationClient(config.AzureADTenantId, config.AzureADApplicationId, config.AzureKeyVaultUrl, config.AzureADAppCertificateKey);
                }
            }
            else
            {
                _authenticationClient = new AuthenticationClient(config.AzureADTenantId, config.AzureADApplicationId, config.AzureADAppClientSecretKey);
            }
        }

        public HttpClient CreateAuthenticatedHttpClient(Uri resource, Guid? uniqueId = null)
        {
            return CreateAuthenticatedHttpClient(resource + ".default", uniqueId);
        }

        public HttpClient CreateAuthenticatedHttpClient(string resource, Guid? uniqueId = null)
        {
            var httpClient = _httpClientFactory.CreateClient(resource + uniqueId?.ToString());
            var authToken = _authenticationClient.GetAuthenticationTokenAsync(resource).Result;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authToken.AccessTokenType, authToken.AccessToken);
            return httpClient;
        }
    }
}