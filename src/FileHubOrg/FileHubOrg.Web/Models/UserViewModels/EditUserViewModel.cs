using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;
using System.ComponentModel.DataAnnotations;

namespace FileHubOrg.Web.Models.UserViewModels
{
    public class EditUserViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public Guid? DepartmentId { get; set; }

        [Display(Name = "Upload Limit (MB)")]
        [Range(0, long.MaxValue, ErrorMessage = "Upload limit must be a positive number or leave empty for unlimited")]
        public long? UploadLimitMB { get; set; }

        [Display(Name = "Account Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Deactivation Reason")]
        public string DeactivationReason { get; set; } = string.Empty;

        [Display(Name = "Deactivated At")]
        public DateTime? DeactivatedAt { get; set; }

        // Read-only display fields
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CurrentDepartmentName { get; set; }

        // Upload statistics
        public int UploadedFilesCount { get; set; } = 0;
        public double TotalUploadedSizeMB { get; set; } = 0;
        public double UploadUsagePercentage { get; set; } = 0;

        [Display(Name = "Last Login")]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "Last Activity")]
        public DateTime? LastActivityAt { get; set; }

        public IReadOnlyList<Department> Departments { get; set; } = new List<Department>();
    }
}
