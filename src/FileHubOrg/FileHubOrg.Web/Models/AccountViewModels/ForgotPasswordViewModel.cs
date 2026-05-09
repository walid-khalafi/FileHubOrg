using System.ComponentModel.DataAnnotations;

namespace FileHubOrg.Web.Models.AccountViewModels
{
    /// <summary>
    /// View model for forgot password functionality
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the email address for password recovery
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
