using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class ApplicationRoleRepository : GenericRepository<ApplicationRole>, IApplicationRoleRepository
    {
        private readonly FileHubOrgDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ApplicationRoleRepository(FileHubOrgDbContext context, RoleManager<ApplicationRole> roleManager) : base(context)
        {
            _context = context;
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<IdentityResult> AddClaimAsync(ApplicationRole role, string claimType, string claimValue, CancellationToken ct = default)
        {
            return await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(claimType, claimValue));
        }

        public async Task<ApplicationRole?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _roleManager.FindByNameAsync(name);
        }

        public async Task<IReadOnlyList<string>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return new List<string>().AsReadOnly();

            var claims = await _roleManager.GetClaimsAsync(role);
            return claims.Select(c => c.Value).ToList().AsReadOnly();
        }

        public async Task<IdentityResult> RemoveClaimAsync(ApplicationRole role, string claimType, string claimValue, CancellationToken ct = default)
        {
            return await _roleManager.RemoveClaimAsync(role, new System.Security.Claims.Claim(claimType, claimValue));
        }

        public async Task<bool> RoleExistsAsync(string name, CancellationToken ct = default)
        {
            return await _roleManager.RoleExistsAsync(name);
        }
    }

}
