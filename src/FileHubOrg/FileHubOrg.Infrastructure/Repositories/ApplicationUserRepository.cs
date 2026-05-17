using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly FileHubOrgDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ApplicationUserRepository(FileHubOrgDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) : base(context)
        {
            _context = context;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<IReadOnlyList<ApplicationUser>> FindAsync(Func<ApplicationUser, bool> predicate, CancellationToken ct = default)
        {
            var users = _userManager.Users.Where(predicate).ToList();
            return await Task.FromResult(users.AsReadOnly());
        }

        public async Task<List<ApplicationUser>?> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        {
            List<ApplicationUser> users = _userManager.Users.Where(x => x.DepartmentId == departmentId).ToList();
            return users;
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId, CancellationToken ct = default)
        {
            var user = await _context.Users.Include(d => d.Department).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {

                throw new Exception("User NotFound!");
            }
            return user;

        }

        public async Task<IReadOnlyList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct = default)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return users.AsReadOnly();
        }

        public async Task<bool> UserExistsAsync(string userName, CancellationToken ct = default)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }
    }
}
