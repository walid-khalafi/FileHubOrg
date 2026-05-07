using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Persistence.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            FileHubOrgDbContext context)
        {
            var departments = context.Departments.ToList();

            // یوزرهای پایه
            var baseUsers = new List<(string roleName, string email, string firstName, string lastName, string roleType)>
            {
                ("Admin", "admin@filehuborg.com", "Walid", "Khalafi", "Admin"),
            };

            foreach (var (roleName, email, firstName, lastName, roleType) in baseUsers)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = firstName,
                        LastName = lastName,
                        DepartmentId = departments.First().Id
                    };

                    var result = await userManager.CreateAsync(user, "P@ssw0rd123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, roleName);
                    }
                }
            }
        }
    }
}
