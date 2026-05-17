using System.Threading;
using System.Threading.Tasks;

namespace FileHubOrg.Web.Services
{
    public interface ILdapAuthenticationService
    {
        Task<LdapUserInfo?> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default);
    }
}
