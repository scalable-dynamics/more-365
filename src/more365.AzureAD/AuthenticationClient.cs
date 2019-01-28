using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace more365.AzureAD
{
    public sealed class AuthenticationClient : IAuthenticationClient
    {
        private const string AuthenticationAuthority = "https://login.microsoftonline.com";

        private readonly AuthenticationContext _context;
        private readonly X509Certificate2 _certificate;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public AuthenticationClient(Guid tenantId, Guid clientId, X509Certificate2 certificate)
            : this(tenantId, clientId)
        {
            _certificate = certificate;
        }

        public AuthenticationClient(Guid tenantId, Guid clientId, string clientSecret)
            : this(tenantId, clientId)
        {
            _clientSecret = clientSecret;
        }

        private AuthenticationClient(Guid tenantId, Guid clientId)
        {
            if (tenantId == Guid.Empty || clientId == Guid.Empty)
            {
                throw new AuthenticationClientException(AuthenticationAuthority, "Invalid Configuration: TenantId and ClientId is required");
            }
            var authority = AuthenticationAuthority.TrimEnd('/') + "/" + tenantId.ToString("D");
            _context = new AuthenticationContext(authority);
            _clientId = clientId.ToString("D");
        }

        public Task<AuthenticationToken> GetAuthenticationTokenAsync(Uri resource)
        {
            return GetAuthenticationTokenAsync(resource.GetLeftPart(UriPartial.Authority));
        }

        public async Task<AuthenticationToken> GetAuthenticationTokenAsync(string resource)
        {
            try
            {
                var authenticationInfo = (_certificate != null
                    ? await _context.AcquireTokenAsync(resource, new ClientAssertionCertificate(_clientId, _certificate))
                    : await _context.AcquireTokenAsync(resource, new ClientCredential(_clientId, _clientSecret)));
                return new AuthenticationToken(authenticationInfo);
            }
            catch (AdalException ex)
            {
                throw new AuthenticationClientException(_context.Authority, "Application: " + _clientId + "\nResource: " + resource + "\nErrorCode" + ex.ErrorCode + "\nAdalException" + ex.Message);
            }
        }
    }
}
