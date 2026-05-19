using FileHubOrg.Domain.Entities.User;

namespace FileHubOrg.Web.Models.UserViewModels
{
    public class UserStorageReportRowViewModel
    {
        public ApplicationUser User { get; set; } = default!;

        public string DepartmentName { get; set; } = "—";

        public bool IsUnlimited { get; set; }

        /// <summary>
        /// Used storage percent. If unlimited -> 0.
        /// </summary>
        public double UsagePercentage { get; set; }

        public double UsedMb { get; set; }
        public double? LimitMb { get; set; }
    }
}

