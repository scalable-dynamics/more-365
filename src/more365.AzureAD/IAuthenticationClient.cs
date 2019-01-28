using more365.AzureAD;
using System;
using System.Threading.Tasks;

namespace more365
{
    public interface IAuthenticationClient
    {
        Task<AuthenticationToken> GetAuthenticationTokenAsync(string resource);

        Task<AuthenticationToken> GetAuthenticationTokenAsync(Uri resource);
    }
}