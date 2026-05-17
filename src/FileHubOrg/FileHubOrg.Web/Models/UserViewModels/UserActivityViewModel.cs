using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;
using System.ComponentModel.DataAnnotations;

namespace FileHubOrg.Web.Models.UserViewModels
{
    public class UserActivityViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string DepartmentName { get; set; } = "—";

        [Display(Name = "Account Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Last Login")]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "Last Activity")]
        public DateTime? LastActivityAt { get; set; }

        [Display(Name = "Files Uploaded")]
        public int UploadedFilesCount { get; set; }

        [Display(Name = "Total Uploaded Size (MB)")]
        public double TotalUploadedSizeMB { get; set; }

        [Display(Name = "Upload Usage")]
        public double UploadUsagePercentage { get; set; }
    }
}
