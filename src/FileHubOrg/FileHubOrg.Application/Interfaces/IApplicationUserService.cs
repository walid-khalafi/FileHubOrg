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

        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<ApplicationUser?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default);

        Task<bool> EnsureRoleExistsAsync(string roleName, string displayName = "", string description = "", CancellationToken cancellationToken = default);

        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default);

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

        /// <summary>Gets all users in the system.</summary>
        Task<IReadOnlyList<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken = default);

        /// <summary>Changes a user's department assignment.</summary>
        Task<bool> ChangeDepartmentAsync(string userId, Guid? departmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the upload limit for a specific user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="uploadLimitBytes">The upload limit in bytes, or null to remove the override.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> SetUserUploadLimitAsync(string userId, long? uploadLimitBytes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates a user account, preventing them from logging in or performing operations.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="reason">The reason for deactivation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        Task<bool> DeactivateUserAsync(string userId, string reason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reactivates a deactivated user account.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        Task<bool> ReactivateUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets user statistics including upload usage information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A tuple containing (uploadedFilesCount, totalUploadedSizeInBytes, uploadLimitBytes)</returns>
        Task<(int fileCount, long totalSizeBytes, long? limitBytes)> GetUserUploadStatsAsync(string userId, CancellationToken cancellationToken = default);
    }
}
