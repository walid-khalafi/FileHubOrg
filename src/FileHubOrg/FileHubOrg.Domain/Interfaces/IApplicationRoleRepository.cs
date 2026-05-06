using FileHubOrg.Domain.Entities.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for ApplicationRole operations.
    /// </summary>
    public interface IApplicationRoleRepository : IGenericRepository<ApplicationRole>
    {

        /// <summary>
        /// Gets a role by its name.
        /// </summary>
        /// <param name="name">The role name.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The role or null if not found.</returns>
        Task<ApplicationRole?> GetByNameAsync(string name, CancellationToken ct = default);

     
        /// <summary>
        /// Checks if a role exists with the specified name.
        /// </summary>
        /// <param name="name">The role name to check.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>True if the role exists; otherwise, false.</returns>
        Task<bool> RoleExistsAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// Gets all permissions/claims for a specific role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A read-only list of claims for the specified role.</returns>
        Task<IReadOnlyList<string>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default);

        /// <summary>
        /// Adds a claim to a role.
        /// </summary>
        /// <param name="role">The role to add the claim to.</param>
        /// <param name="claimType">The claim type.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The identity result of the operation.</returns>
        Task<IdentityResult> AddClaimAsync(ApplicationRole role, string claimType, string claimValue, CancellationToken ct = default);

        /// <summary>
        /// Removes a claim from a role.
        /// </summary>
        /// <param name="role">The role to remove the claim from.</param>
        /// <param name="claimType">The claim type.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The identity result of the operation.</returns>
        Task<IdentityResult> RemoveClaimAsync(ApplicationRole role, string claimType, string claimValue, CancellationToken ct = default);
    }
}
