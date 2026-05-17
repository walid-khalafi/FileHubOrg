using FileHubOrg.Application.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace FileHubOrg.Web.Services
{
    public class LdapUserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public string DistinguishedName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class LdapAuthenticationService : ILdapAuthenticationService
    {
        private readonly LdapSettings _settings;
        private readonly ILogger<LdapAuthenticationService> _logger;

        public LdapAuthenticationService(IOptions<LdapSettings> settings, ILogger<LdapAuthenticationService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public Task<LdapUserInfo?> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                return Task.FromResult<LdapUserInfo?>(null);
            }

            if (string.IsNullOrWhiteSpace(_settings.Url) || string.IsNullOrWhiteSpace(_settings.SearchBase))
            {
                _logger.LogWarning("LDAP configuration is incomplete; LDAP login is disabled.");
                return Task.FromResult<LdapUserInfo?>(null);
            }

            try
            {
                var entry = FindUserEntry(userName);
                if (entry == null)
                {
                    return Task.FromResult<LdapUserInfo?>(null);
                }

                var isValid = ValidateCredentials(entry.DN, password);
                if (!isValid)
                {
                    return Task.FromResult<LdapUserInfo?>(null);
                }

                var userInfo = BuildUserInfo(entry, userName);
                return Task.FromResult<LdapUserInfo?>(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LDAP authentication failed for user {UserName}.", userName);
                return Task.FromResult<LdapUserInfo?>(null);
            }
        }

        private LdapEntry? FindUserEntry(string userName)
        {
            using var connection = CreateConnection();
            connection.Connect(_settings.Url, _settings.Port);
            if (!string.IsNullOrWhiteSpace(_settings.BindOn))
            {
                connection.Bind(_settings.BindOn, _settings.BindCredentials);
            }

            var filter = string.Format(_settings.SearchFilter, EscapeLdapFilterValue(userName));
            var search = connection.Search(
                _settings.SearchBase,
                LdapConnection.SCOPE_SUB,
                filter,
                null,
                false);

            if (search.HasMore())
            {
                return search.Next();
            }

            return null;
        }

        private bool ValidateCredentials(string distinguishedName, string password)
        {
            using var connection = CreateConnection();
            connection.Connect(_settings.Url, _settings.Port);
            connection.Bind(distinguishedName, password);
            return connection.Bound;
        }

        private LdapConnection CreateConnection()
        {
            return new LdapConnection { SecureSocketLayer = _settings.UseSsl };
        }

        private static string EscapeLdapFilterValue(string value)
        {
            return value
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }

        private LdapUserInfo BuildUserInfo(LdapEntry entry, string userName)
        {
            var email = GetAttributeValue(entry, _settings.EmailNameAttribute);
            var displayName = GetAttributeValue(entry, _settings.DisplayNameAttribute);

            return new LdapUserInfo
            {
                UserName = userName,
                DistinguishedName = entry.DN,
                Email = email,
                DisplayName = displayName
            };
        }

        private static string GetAttributeValue(LdapEntry entry, string attributeName)
        {
            var attribute = entry.getAttribute(attributeName);
            return attribute?.StringValue ?? string.Empty;
        }
    }
}
