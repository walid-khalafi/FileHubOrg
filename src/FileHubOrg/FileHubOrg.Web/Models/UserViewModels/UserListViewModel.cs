using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;
using System.Collections.Generic;

namespace FileHubOrg.Web.Models.UserViewModels
{
    public class UserListViewModel
    {
        public IReadOnlyList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public IReadOnlyList<Department> Departments { get; set; } = new List<Department>();
        public IReadOnlyDictionary<string, int> UserFileCounts { get; set; } = new Dictionary<string, int>();
    }
}
