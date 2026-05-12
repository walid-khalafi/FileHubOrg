using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<List<Department>> GetAllAsync();
    }
}
