using FileHubOrg.Domain.Entities.Organization;
using Microsoft.AspNetCore.Identity;

namespace FileHubOrg.Domain.Entities.User
{
    /// <summary>
    /// Represents an application user in the system that extends the ASP.NET Core Identity framework.
    /// This class contains additional user profile information beyond the base IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        /// <value>The first name of the user.</value>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        /// <value>The last name of the user.</value>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's avatar image URL or base64 string.
        /// </summary>
        /// <value>The avatar image URL or base64 encoded image data.</value>
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// Gets the user's full name by combining FirstName and LastName.
        /// This is a read-only computed property that automatically handles trimming.
        /// </summary>
        /// <value>The full name in the format "FirstName LastName".</value>
        /// <example>
        /// If FirstName = "John" and LastName = "Doe", FullName returns "John Doe"
        /// If FirstName = "John" and LastName is empty, FullName returns "John"
        /// </example>
        public string FullName => $"{FirstName} {LastName}".Trim();

        public Guid? DepartmentId { get;  set; }
        public virtual Department? Department { get; set; }

        /// <summary>
        /// Gets or sets the maximum upload limit in bytes for this user.
        /// If null, the system default limit applies.
        /// </summary>
        /// <value>The upload limit in bytes, or null for no override.</value>
        public long? UploadLimitBytes { get; set; }

        /// <summary>
        /// Gets or sets whether the user account is active.
        /// Deactivated accounts cannot log in or perform any operations.
        /// </summary>
        /// <value>True if the account is active; otherwise, false.</value>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the date and time when the user account was deactivated.
        /// Null if the account is active or has never been deactivated.
        /// </summary>
        /// <value>The deactivation date, or null.</value>
        public DateTime? DeactivatedAt { get; set; }

        /// <summary>
        /// Gets or sets the reason for account deactivation.
        /// </summary>
        /// <value>The deactivation reason, or empty string.</value>
        public string DeactivationReason { get; set; } = string.Empty;
    }
}
