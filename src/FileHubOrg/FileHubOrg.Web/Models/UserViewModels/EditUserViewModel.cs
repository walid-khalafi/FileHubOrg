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

        // Read-only display fields
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CurrentDepartmentName { get; set; }

        public IReadOnlyList<Department> Departments { get; set; } = new List<Department>();
    }
}
