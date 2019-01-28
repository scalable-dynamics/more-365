using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace more365.AzureAD
{
    public sealed class AuthenticationToken
    {
        public string AccessTokenType { get; }

        public string AccessToken { get; }

        public string RefreshToken { get; }

        public DateTimeOffset ExpiresOn { get; }

        internal AuthenticationToken(AuthenticationResult authenticationResult)
        {
            AccessTokenType = authenticationResult.AccessTokenType;
            AccessToken = authenticationResult.AccessToken;
            ExpiresOn = authenticationResult.ExpiresOn;
        }

        private AuthenticationToken() { }
    }
}
