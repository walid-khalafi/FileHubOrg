using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<IReadOnlyList<Department>> GetDepartmentsAsync();
        Task<Department> GetDepartmentsByIdAsync(Guid id);
        Task<List<ApplicationUser>> GetDepartmentUsersAsync(Guid id);
    }
}
