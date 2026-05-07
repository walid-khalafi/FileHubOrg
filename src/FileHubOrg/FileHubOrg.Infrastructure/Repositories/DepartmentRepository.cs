using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        private readonly FileHubOrgDbContext _context;
        public DepartmentRepository(FileHubOrgDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
