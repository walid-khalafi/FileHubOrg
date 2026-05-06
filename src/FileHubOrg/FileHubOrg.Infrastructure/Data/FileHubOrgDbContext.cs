using FileHubOrg.Domain.Entities.File;
using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.Token;
using FileHubOrg.Domain.Entities.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Data
{
    public class FileHubOrgDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public FileHubOrgDbContext(DbContextOptions<FileHubOrgDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<FileMetaData> FileMetaData { get; set; }
        public DbSet<JWT> JWTs { get; set; }
    }
}
