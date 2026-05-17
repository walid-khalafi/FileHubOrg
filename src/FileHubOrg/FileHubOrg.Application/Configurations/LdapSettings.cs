namespace FileHubOrg.Application.Configurations
{
    public class LdapSettings
    {
        public string Url { get; set; } = string.Empty;
        public int Port { get; set; } = 389;
        public bool UseSsl { get; set; }
        public string BindOn { get; set; } = string.Empty;
        public string BindCredentials { get; set; } = string.Empty;
        public string SearchFilter { get; set; } = "(&(objectClass=*)(sAMAccountName={0}))";
        public string SearchBase { get; set; } = string.Empty;
        public string UIDAttribute { get; set; } = "objectGUID";
        public string SAMAccountNameAttribute { get; set; } = "sAMAccountName";
        public string DisplayNameAttribute { get; set; } = "CN";
        public string EmailNameAttribute { get; set; } = "mail";
    }
}
