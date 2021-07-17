using Microsoft.Identity.Client;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace more365.AzureAD
{
    public sealed class AuthenticationClient : IAuthenticationClient
    {
        private string AuthenticationAuthority => $"https://login.microsoftonline.com/{_tenantId}";

        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly IConfidentialClientApplication _app;

        private AuthenticationClient(Guid tenantId, Guid clientId)
        {
            if (tenantId == Guid.Empty || clientId == Guid.Empty)
            {
                throw new AuthenticationClientException(AuthenticationAuthority, "Invalid Configuration: TenantId and ClientId is required");
            }
            _tenantId = tenantId.ToString();
            _clientId = clientId.ToString("D");
        }

        public AuthenticationClient(Guid tenantId, Guid clientId, X509Certificate2 certificate)
            : this(tenantId, clientId)
        {
            _app = ConfidentialClientApplicationBuilder.Create(_clientId)
                                                       .WithAuthority(AuthenticationAuthority)
                                                       .WithCertificate(certificate)
                                                       .Build();
        }

        public AuthenticationClient(Guid tenantId, Guid clientId, string clientSecret)
            : this(tenantId, clientId)
        {
            _app = ConfidentialClientApplicationBuilder.Create(_clientId)
                                                       .WithAuthority(AuthenticationAuthority)
                                                       .WithClientSecret(clientSecret)
                                                       .Build();
        }

        public Task<AuthenticationToken> GetAuthenticationTokenAsync(Uri resource)
        {
            return GetAuthenticationTokenAsync(resource.GetLeftPart(UriPartial.Authority) + "/.default");
        }

        public async Task<AuthenticationToken> GetAuthenticationTokenAsync(string resource)
        {
            try
            {
                var result = await _app.AcquireTokenForClient(new[] { resource })
                                       .ExecuteAsync();
                return new AuthenticationToken(result);
            }
            catch (MsalException ex)
            {
                throw new AuthenticationClientException(AuthenticationAuthority, $@"
Application: {_clientId}
Resource: {resource}
ErrorCode: {ex.ErrorCode}
MsalException: {ex.Message}
");
            }
        }
    }
}
