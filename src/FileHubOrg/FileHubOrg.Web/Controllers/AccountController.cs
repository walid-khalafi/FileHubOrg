using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Infrastructure.Data;
using FileHubOrg.Web.Models.AccountViewModels;
using FileHubOrg.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace FileHubOrg.Web.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILdapAuthenticationService _ldapAuthenticationService;

        private readonly ILogger _logger;
        private readonly FileHubOrgDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountController(
                    UserManager<ApplicationUser> userManager,
                    SignInManager<ApplicationUser> signInManager,
                    ILdapAuthenticationService ldapAuthenticationService,
                    ILogger<AccountController> logger,
                    FileHubOrgDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _ldapAuthenticationService = ldapAuthenticationService;
            _logger = logger;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }


        [TempData]
        public string ErrorMessage { get; set; }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }


        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = "")
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This counts login failures towards account lockout
                // To disable password failures to trigger account lockout, set lockoutOnFailure: false
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in successfully.");

                    return RedirectToLocal(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out due to multiple failed login attempts.");
                    return RedirectToAction(nameof(Lockout));
                }

                var ldapUser = await _ldapAuthenticationService.AuthenticateAsync(model.UserName, model.Password);
                if (ldapUser != null)
                {
                    var user = await GetOrCreateLdapUserAsync(ldapUser);
                    if (user != null)
                    {
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        _logger.LogInformation("User logged in successfully via LDAP.");
                        return RedirectToLocal(returnUrl);
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid username or password. Please check your credentials and try again.");
                return View(model);
            }

            _logger.LogWarning("Invalid login attempt due to invalid model state.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in successfully with 2FA.", user.Id);

                return RedirectToLocal(returnUrl);

            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out due to failed 2FA attempts.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code. Please check your authenticator app and try again.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in successfully with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out due to failed recovery code attempts.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code. Please check your backup codes and try again.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully.");
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in successfully with {Name} provider.", info.LoginProvider);

                return RedirectToLocal(returnUrl);

            }

            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    // Ensure Client role exists for new external login users
                    var role = _context.Roles.FirstOrDefault(x => x.Name == "Client");
                    if (role == null)
                    {
                        _context.Roles.Add(new ApplicationRole
                        {
                            ConcurrencyStamp = Guid.NewGuid().ToString(),
                            Id = Guid.NewGuid().ToString(),
                            Name = "Client",
                            NormalizedName = "CLIENT",
                        });
                        _context.SaveChanges();
                    }

                    await _userManager.AddToRoleAsync(user, "Client");
                    result = await _userManager.AddLoginAsync(user, info);

                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account successfully using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }

                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                // var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                // await _emailSender.SendEmailAsync("support@chaptak.com", model.Email, "Reset Password",
                // $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<ApplicationUser?> GetOrCreateLdapUserAsync(LdapUserInfo ldapUser)
        {
            var user = await _userManager.FindByNameAsync(ldapUser.UserName);

            if (user == null && !string.IsNullOrWhiteSpace(ldapUser.Email))
            {
                user = await _userManager.FindByEmailAsync(ldapUser.Email);
            }

            if (user == null)
            {
                var (firstName, lastName) = SplitDisplayName(ldapUser.DisplayName);
                user = new ApplicationUser
                {
                    UserName = ldapUser.UserName,
                    Email = $"{ldapUser.UserName}@filehuborg.com",
                    EmailConfirmed = !string.IsNullOrWhiteSpace(ldapUser.Email),
                    FirstName = firstName,
                    LastName = lastName
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _logger.LogWarning("Unable to create local user account for LDAP user {UserName}. Errors: {Errors}", ldapUser.UserName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return null;
                }

                var role = _context.Roles.FirstOrDefault(x => x.Name == "Client");
                if (role == null)
                {
                    _context.Roles.Add(new ApplicationRole
                    {
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        Id = Guid.NewGuid().ToString(),
                        Name = "Client",
                        NormalizedName = "CLIENT",
                    });
                    _context.SaveChanges();
                }

                await _userManager.AddToRoleAsync(user, "Client");
            }

            // Assign user to department based on LDAP OU
            await AssignUserToDepartmentByOUAsync(user, ldapUser.DistinguishedName);

            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(x => x.LoginProvider == "LDAP" && x.ProviderKey == ldapUser.UserName))
            {
                await _userManager.AddLoginAsync(user, new UserLoginInfo("LDAP", ldapUser.UserName, "LDAP"));
            }

            return user;
        }

        private static (string FirstName, string LastName) SplitDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return (string.Empty, string.Empty);
            }

            var parts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return (parts[0], string.Empty);
            }

            return (parts[0], string.Join(' ', parts.Skip(1)));
        }

        private async Task AssignUserToDepartmentByOUAsync(ApplicationUser user, string distinguishedName)
        {
            try
            {
                var ouName = ExtractOUFromDN(distinguishedName);
                if (string.IsNullOrWhiteSpace(ouName))
                {
                    _logger.LogWarning("Could not extract OU from DN: {DN}", distinguishedName);
                    return;
                }

                var department = _context.Departments.FirstOrDefault(d => d.Name == ouName);
                if (department == null)
                {
                    department = new Domain.Entities.Organization.Department
                    {
                        Id = Guid.NewGuid(),
                        Name = ouName,
                        CreatedBy = "System",
                        CreatedByIP = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown"
                    };
                    _context.Departments.Add(department);
                    _context.SaveChanges();
                    _logger.LogInformation("Created new department from LDAP OU: {DepartmentName}", ouName);
                }

                if (user.DepartmentId != department.Id)
                {
                    user.DepartmentId = department.Id;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("Assigned user {UserName} to department {DepartmentName}", user.UserName, department.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user {UserName} to department based on OU", user.UserName);
            }
        }

        private static string ExtractOUFromDN(string distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                return string.Empty;
            }

            // DN format: CN=username,OU=Department,OU=SubOU,DC=example,DC=com
            // We want to extract the first OU (the immediate organizational unit)
            var parts = distinguishedName.Split(',');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("OU=", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the value after "OU="
                    return trimmed.Substring(3);
                }
            }

            return string.Empty;
        }

        #region Helpers

        /// <summary>
        /// Adds Identity errors to ModelState for display to user
        /// </summary>
        /// <param name="result">IdentityResult containing errors to display</param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        /// <summary>
        /// Redirects to local URL if valid, otherwise redirects to home page
        /// </summary>
        /// <param name="returnUrl">URL to redirect to</param>
        /// <returns>Redirect result</returns>
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion

    }
}
