using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using more365.AzureAD;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

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
            _authenticationClient = createAuthenticationClient(config).Result;
        }

        public HttpClient CreateAuthenticatedHttpClient(string resource, Guid? uniqueId = null)
        {
            var httpClient = _httpClientFactory.CreateClient(resource + uniqueId?.ToString());
            var authToken = _authenticationClient.GetAuthenticationTokenAsync(resource).Result;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authToken.AccessTokenType, authToken.AccessToken);
            return httpClient;
        }

        private static async Task<IAuthenticationClient> createAuthenticationClient(More365Configuration config)
        {
            if (!string.IsNullOrWhiteSpace(config.AzureADAppCertificateKey) && config.AzureADAppCertificateKey.Contains("vault.azure.net"))
            {
                var cert = await getSecretFromKey(config.AzureADAppCertificateKey);
                var certificate = new X509Certificate2(Convert.FromBase64String(cert));
                return new AuthenticationClient(config.AzureADTenantId, config.AzureADApplicationId, certificate);
            }
            else if (!string.IsNullOrWhiteSpace(config.AzureADAppClientSecretKey) && config.AzureADAppClientSecretKey.Contains("vault.azure.net"))
            {
                var clientSecret = await getSecretFromKey(config.AzureADAppClientSecretKey);
                return new AuthenticationClient(config.AzureADTenantId, config.AzureADApplicationId, clientSecret);
            }
            else if (!string.IsNullOrWhiteSpace(config.AzureADAppClientSecretKey))
            {
                return new AuthenticationClient(config.AzureADTenantId, config.AzureADApplicationId, config.AzureADAppClientSecretKey);
            }
            else
            {
                throw new More365Exception("The application encountered an error while retrieving configuration for more365", "AzureADAppCertificateKey or AzureADAppClientSecretKey is required");
            }
        }

        private static async Task<string> getSecretFromKey(string key)
        {
            var serviceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync(key);
            return secret.Value;
        }
    }
}