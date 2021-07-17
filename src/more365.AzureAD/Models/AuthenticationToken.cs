using Microsoft.Identity.Client;
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
            AccessTokenType = authenticationResult.TokenType;
            AccessToken = authenticationResult.AccessToken;
            ExpiresOn = authenticationResult.ExpiresOn;
        }

        private AuthenticationToken() { }
    }
}
