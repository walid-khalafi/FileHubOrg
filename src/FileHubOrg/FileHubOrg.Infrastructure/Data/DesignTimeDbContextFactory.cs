using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FileHubOrgDbContext>
    {
        public FileHubOrgDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

            var dbType = configuration["DatabaseType"];
            string connectionString;

            if (dbType == "MySQL")
            {
                connectionString = configuration.GetConnectionString("MySQLConnection")
                    ?? "Server=localhost;Database=FileHubOrgDB;Uid=root;Pwd=password;Convert Zero Datetime=True";
            }
            else
            {
                connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\mssqllocaldb;Database=FileHubOrgDB;Trusted_Connection=True;MultipleActiveResultSets=true";
            }

            var optionsBuilder = new DbContextOptionsBuilder<FileHubOrgDbContext>();

            if (dbType == "MySQL")
            {
                optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
            }
            else
            {
                optionsBuilder.UseSqlServer(connectionString);
            }

            return new FileHubOrgDbContext(optionsBuilder.Options);
        }
    }
}
