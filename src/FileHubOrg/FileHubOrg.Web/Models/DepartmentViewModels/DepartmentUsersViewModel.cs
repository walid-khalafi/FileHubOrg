using FileHubOrg.Domain.Entities.User;

namespace FileHubOrg.Web.Models.DepartmentViewModels
{
    public class DepartmentUsersViewModel
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public List<ApplicationUser> Users { get; set; } = new();
    }
}
