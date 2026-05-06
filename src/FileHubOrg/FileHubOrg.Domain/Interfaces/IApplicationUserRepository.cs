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
    /// Repository interface for ApplicationUser operations.
    /// </summary>
    public interface IApplicationUserRepository : IGenericRepository<ApplicationUser>
    {


        /// <summary>
        /// Gets a user by their username.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The user or null if not found.</returns>
        Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken ct = default);

        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The user or null if not found.</returns>
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Finds users matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter users.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A read-only list of matching users.</returns>
        Task<IReadOnlyList<ApplicationUser>> FindAsync(Func<ApplicationUser, bool> predicate, CancellationToken ct = default);

       
        /// <summary>
        /// Checks if a user exists with the specified username.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>True if the user exists; otherwise, false.</returns>
        Task<bool> UserExistsAsync(string userName, CancellationToken ct = default);

        /// <summary>
        /// Checks if a user exists with the specified email.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>True if the user exists; otherwise, false.</returns>
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Gets users in a specific role.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A read-only list of users in the specified role.</returns>
        Task<IReadOnlyList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct = default);
    }
}
