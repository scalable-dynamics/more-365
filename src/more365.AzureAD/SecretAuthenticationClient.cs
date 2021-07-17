using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace more365.AzureAD
{
    public sealed class SecretAuthenticationClient : IAuthenticationClient
    {
        public static string GetKeyVaultUrl(string keyVaultName) => $"https://{keyVaultName}.vault.azure.net/";

        private readonly Guid tenantId;
        private readonly Guid clientId;
        private readonly Uri keyVaultUrl;
        private readonly string secretName;
        private readonly bool isCertificate;

        public SecretAuthenticationClient(Guid tenantId, Guid clientId, Uri keyVaultUrl, string secretName, bool isCertificate = false)
        {
            this.tenantId = tenantId;
            this.clientId = clientId;
            this.keyVaultUrl = keyVaultUrl;
            this.secretName = secretName;
            this.isCertificate = isCertificate;
        }

        public async Task<AuthenticationToken> GetAuthenticationTokenAsync(Uri resource)
        {
            var client = await getAuthenticationClient();
            return await client.GetAuthenticationTokenAsync(resource);
        }

        public async Task<AuthenticationToken> GetAuthenticationTokenAsync(string resource)
        {
            var client = await getAuthenticationClient();
            return await client.GetAuthenticationTokenAsync(resource);
        }

        private async Task<IAuthenticationClient> getAuthenticationClient()
        {
            var secretClient = new SecretClient(keyVaultUrl, new DefaultAzureCredential());
            var secretResponse = await secretClient.GetSecretAsync(secretName);
            if (isCertificate)
            {
                var certificate = new X509Certificate2(Convert.FromBase64String(secretResponse.Value.Value));
                return new AuthenticationClient(tenantId, clientId, certificate);
            }
            else
            {
                return new AuthenticationClient(tenantId, clientId, secretResponse.Value.Value);
            }
        }
    }
}
