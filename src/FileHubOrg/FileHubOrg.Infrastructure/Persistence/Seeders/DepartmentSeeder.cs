using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Persistence.Seeders
{
    public static class DepartmentSeeder
    {
        public static async Task SeedAsync(FileHubOrgDbContext context)
        {
            if (!context.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department { Id = Guid.NewGuid(), Name = "Planning", CreatedAt = DateTime.Now, CreatedBy = Guid.Empty.ToString(), CreatedByIP = "127.0.0.1" },
                    new Department { Id = Guid.NewGuid(), Name = "Technical",CreatedAt = DateTime.Now, CreatedBy = Guid.Empty.ToString(), CreatedByIP = "127.0.0.1" },
                    new Department { Id = Guid.NewGuid(), Name = "Human Resources", CreatedAt = DateTime.Now, CreatedBy = Guid.Empty.ToString(), CreatedByIP = "127.0.0.1"  },
                    new Department { Id = Guid.NewGuid(), Name = "CEO",   CreatedAt = DateTime.Now, CreatedBy = Guid.Empty.ToString(), CreatedByIP = "127.0.0.1" },
                    new Department { Id = Guid.NewGuid(), Name = "Security",    CreatedAt = DateTime.Now, CreatedBy = Guid.Empty.ToString(), CreatedByIP = "127.0.0.1"  },
                    new Department { Id = Guid.NewGuid(), Name = "HSEE", CreatedAt = DateTime.Now, CreatedBy = Guid.Empty.ToString(), CreatedByIP = "127.0.0.1" }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }
        }
    }
}
