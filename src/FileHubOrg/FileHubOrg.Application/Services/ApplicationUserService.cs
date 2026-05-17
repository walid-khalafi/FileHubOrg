using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Services
{
    /// <summary>
    /// Service implementation for ApplicationUser operations.
    /// Handles business logic for user profile management.
    /// </summary>
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserService"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public ApplicationUserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        /// <inheritdoc />
        public async Task<ApplicationUser?> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            return await _unitOfWork.ApplicationUsers.GetUserByIdAsync(userId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ChangeUserFullNameAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
            }

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();

            try
            {
                await _unitOfWork.ApplicationUsers.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
          
        }

        /// <inheritdoc />
        public async Task<bool> ChangeUserAvatarAsync(string userId, string avatar, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(avatar))
            {
                throw new ArgumentException("Avatar cannot be null or empty.", nameof(avatar));
            }

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.Avatar = avatar.Trim();

            try
            {
                await _unitOfWork.ApplicationUsers.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
           
          
          
        }

        /// <inheritdoc />
        public async Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("User Not Found.", nameof(userId));
            }

            user.TwoFactorEnabled = enabled;
            try
            {
              
                await _unitOfWork.ApplicationUsers.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        /// <inheritdoc />
        public async Task<IReadOnlyList<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.ApplicationUsers.FindAsync(_ => true, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            return await _userManager.CreateAsync(user, password);
        }

        /// <inheritdoc />
        public async Task<bool> EnsureRoleExistsAsync(string roleName, string displayName = "", string description = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return true;
            }

            var role = new ApplicationRole
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? roleName : displayName,
                Description = description ?? string.Empty
            };

            var result = await _roleManager.CreateAsync(role);
            return result.Succeeded;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        /// <inheritdoc />
        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            return await _unitOfWork.ApplicationUsers.GetByEmailAsync(email, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ApplicationUser?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
            }

            return await _unitOfWork.ApplicationUsers.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ChangeDepartmentAsync(string userId, Guid? departmentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            user.DepartmentId = departmentId;

            try
            {
                await _unitOfWork.ApplicationUsers.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SetUserUploadLimitAsync(string userId, long? uploadLimitBytes, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            if (uploadLimitBytes.HasValue && uploadLimitBytes <= 0)
                throw new ArgumentException("Upload limit must be greater than 0.", nameof(uploadLimitBytes));

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.UploadLimitBytes = uploadLimitBytes;

            try
            {
                await _unitOfWork.ApplicationUsers.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeactivateUserAsync(string userId, string reason, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Deactivation reason cannot be null or empty.", nameof(reason));

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.IsActive = false;
            user.DeactivatedAt = DateTime.UtcNow;
            user.DeactivationReason = reason.Trim();

            try
            {
                await _unitOfWork.ApplicationUsers.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ReactivateUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.IsActive = true;
            user.DeactivatedAt = null;
            user.DeactivationReason = string.Empty;

            try
            {
                await _unitOfWork.ApplicationUsers.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<(int fileCount, long totalSizeBytes, long? limitBytes)> GetUserUploadStatsAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return (0, 0, null);

            // Get all files created by this user
            var userFiles = await _unitOfWork.FileMetaData.FindAsync(f => f.CreatedBy == userId, cancellationToken);
            
            var fileCount = userFiles.Count;
            var totalSizeBytes = userFiles.Sum(f => f.Size);
            var uploadLimit = user.UploadLimitBytes;

            return (fileCount, totalSizeBytes, uploadLimit);
        }
    }
}