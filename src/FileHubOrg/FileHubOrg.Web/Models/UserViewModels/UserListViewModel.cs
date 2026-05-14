using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;

namespace FileHubOrg.Web.Models.UserViewModels
{
    public class UserListViewModel
    {
        public IReadOnlyList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public IReadOnlyList<Department> Departments { get; set; } = new List<Department>();
    }
}
