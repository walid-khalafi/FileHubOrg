using FileHubOrg.Domain.Entities.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Interfaces
{
    /// <summary>
    /// Service interface for ApplicationUser operations.
    /// Provides business logic layer for user profile management.
    /// </summary>
    public interface IApplicationUserService
    {
        /// <summary>
        /// Gets a user profile by user ID.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The user profile or null if not found.</returns>
        Task<ApplicationUser?> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a user's full name.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="firstName">The user's first name.</param>
        /// <param name="lastName">The user's last name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> ChangeUserFullNameAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a user's avatar.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="avatar">The avatar data or URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> ChangeUserAvatarAsync(string userId, string avatar, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a user's two-factor authentication setting.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="enabled">Whether two-factor authentication should be enabled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled, CancellationToken cancellationToken = default);
    }
}
