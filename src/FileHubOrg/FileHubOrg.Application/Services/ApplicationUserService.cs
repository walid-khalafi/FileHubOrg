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

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserService"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance.</param>
        public ApplicationUserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <inheritdoc />
        public async Task<ApplicationUser?> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            return await _unitOfWork.ApplicationUsers.GetByIdAsync(userId, cancellationToken);
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
    }
}