using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Web.Models.AccountViewModels;
using FileHubOrg.Web.Models.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace FileHubOrg.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IApplicationUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IApplicationUserService userService,
            IDepartmentService departmentService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _departmentService = departmentService;
            _logger = logger;
        }

        // GET /User
        public async Task<IActionResult> Index()
        {
            var users       = await _userService.GetAllUsersAsync();
            var departments = await _departmentService.GetDepartmentsAsync();

            var model = new UserListViewModel
            {
                Users       = users,
                Departments = departments
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register(string returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                TempData["error_msg"] = "Please correct the errors in the registration form and try again.";
                return View(model);
            }

            try
            {
                var existingUserByPhone = await _userService.GetByPhoneNumberAsync(model.PhoneNumber);
                if (existingUserByPhone != null)
                {
                    TempData["error_msg"] = "This phone number is already registered. If you've forgotten your password, please use the password recovery option.";
                    return View(model);
                }

                var existingUserByEmail = await _userService.GetByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    TempData["error_msg"] = "This email address is already registered. If you've forgotten your password, please use the password recovery option.";
                    return View(model);
                }

                var newUser = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var createResult = await _userService.CreateUserAsync(newUser, model.Password);

                if (createResult.Succeeded)
                {
                    await _userService.EnsureRoleExistsAsync("Client", "Client", "Standard client user role");
                    await _userService.AddToRoleAsync(newUser, "Client");
                    _logger.LogInformation("New user account created successfully for user: {UserId}", newUser.Id);
                    TempData["success_msg"] = "Registration successful! Please check your email for account activation instructions.";
                    return RedirectToAction("Index");
                }

                AddErrors(createResult);
                TempData["error_msg"] = "Registration failed. Please review the errors below and try again.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                TempData["error_msg"] = "An unexpected error occurred during registration. Please try again later.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            var model = new CreateUserViewModel
            {
                Departments = departments
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user = await _userService.GetUserProfileAsync(id);
            if (user == null)
                return NotFound();

            var model = new ResetUserPasswordViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetUserPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resetResult = await _userService.ResetUserPasswordAsync(model.UserId, model.Password);
            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            TempData["success_msg"] = "Password was reset successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await _departmentService.GetDepartmentsAsync();
                return View(model);
            }

            try
            {
                var existingUserByPhone = await _userService.GetByPhoneNumberAsync(model.PhoneNumber);
                if (existingUserByPhone != null)
                {
                    ModelState.AddModelError(nameof(model.PhoneNumber), "This phone number is already registered.");
                    model.Departments = await _departmentService.GetDepartmentsAsync();
                    return View(model);
                }

                var existingUserByEmail = await _userService.GetByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "This email address is already registered.");
                    model.Departments = await _departmentService.GetDepartmentsAsync();
                    return View(model);
                }

                var newUser = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    DepartmentId = model.DepartmentId
                };

                var createResult = await _userService.CreateUserAsync(newUser, model.Password);

                if (createResult.Succeeded)
                {
                    await _userService.EnsureRoleExistsAsync("Client", "Client", "Standard client user role");
                    await _userService.AddToRoleAsync(newUser, "Client");
                    _logger.LogInformation("New user account created successfully for user: {UserId}", newUser.Id);
                    TempData["success_msg"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                AddErrors(createResult);
                model.Departments = await _departmentService.GetDepartmentsAsync();
                TempData["error_msg"] = "Unable to create user. Please fix the errors below and try again.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new user");
                model.Departments = await _departmentService.GetDepartmentsAsync();
                TempData["error_msg"] = "An unexpected error occurred while creating the user. Please try again later.";
                return View(model);
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // GET /User/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user        = await _userService.GetUserProfileAsync(id);
            var departments = await _departmentService.GetDepartmentsAsync();

            if (user == null)
                return NotFound();

            // Get upload statistics
            var (fileCount, totalSizeBytes, limitBytes) = await _userService.GetUserUploadStatsAsync(id);

            var model = new EditUserViewModel
            {
                UserId                   = user.Id,
                FirstName                = user.FirstName,
                LastName                 = user.LastName,
                Email                    = user.Email ?? string.Empty,
                PhoneNumber              = user.PhoneNumber ?? string.Empty,
                DepartmentId             = user.DepartmentId,
                CurrentDepartmentName    = user.Department?.Name,
                UploadLimitMB            = limitBytes.HasValue ? limitBytes.Value / (1024 * 1024) : null,
                IsActive                 = user.IsActive,
                DeactivationReason       = user.DeactivationReason,
                DeactivatedAt            = user.DeactivatedAt,
                UploadedFilesCount       = fileCount,
                TotalUploadedSizeMB      = totalSizeBytes / (1024.0 * 1024.0),
                UploadUsagePercentage    = limitBytes.HasValue ? (totalSizeBytes / (double)limitBytes.Value) * 100 : 0,
                Departments              = departments
            };

            return View(model);
        }

        // POST /User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await _departmentService.GetDepartmentsAsync();
                return View(model);
            }

            var nameUpdated = await _userService.ChangeUserFullNameAsync(
                model.UserId, model.FirstName, model.LastName);

            var deptUpdated = await _userService.ChangeDepartmentAsync(
                model.UserId, model.DepartmentId);

            if (nameUpdated && deptUpdated)
                TempData["success_msg"] = $"{model.FirstName} {model.LastName} updated successfully.";
            else
                TempData["error_msg"] = "An error occurred while saving changes.";

            return RedirectToAction(nameof(Index));
        }

        // POST /User/ChangeDepartment  (AJAX)
        [HttpPost]
        public async Task<IActionResult> ChangeDepartment([FromBody] ChangeDepartmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "Invalid user ID." });

            var success = await _userService.ChangeDepartmentAsync(request.UserId, request.DepartmentId);

            if (!success)
                return StatusCode(500, new { message = "Failed to update department." });

            return Ok(new { message = "Department updated." });
        }

        // POST /User/SetUploadLimit (AJAX)
        [HttpPost]
        public async Task<IActionResult> SetUploadLimit([FromBody] SetUploadLimitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "Invalid user ID." });

            if (request.UploadLimitMB.HasValue && request.UploadLimitMB <= 0)
                return BadRequest(new { message = "Upload limit must be greater than 0." });

            long? uploadLimitBytes = null;
            if (request.UploadLimitMB.HasValue)
            {
                uploadLimitBytes = request.UploadLimitMB.Value * 1024 * 1024; // Convert MB to bytes
            }

            var success = await _userService.SetUserUploadLimitAsync(request.UserId, uploadLimitBytes);

            if (!success)
                return StatusCode(500, new { message = "Failed to update upload limit." });

            return Ok(new { message = "Upload limit updated successfully." });
        }

        // POST /User/DeactivateAccount (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeactivateAccount([FromBody] DeactivateAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "Invalid user ID." });

            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { message = "Deactivation reason is required." });

            var success = await _userService.DeactivateUserAsync(request.UserId, request.Reason);

            if (!success)
                return StatusCode(500, new { message = "Failed to deactivate user account." });

            return Ok(new { message = "User account deactivated successfully." });
        }

        // POST /User/ReactivateAccount (AJAX)
        [HttpPost]
        public async Task<IActionResult> ReactivateAccount([FromBody] ReactivateAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "Invalid user ID." });

            var success = await _userService.ReactivateUserAsync(request.UserId);

            if (!success)
                return StatusCode(500, new { message = "Failed to reactivate user account." });

            return Ok(new { message = "User account reactivated successfully." });
        }

        // POST /User/DeleteUser (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "Invalid user ID." });

            var result = await _userService.DeleteUserAsync(request.UserId);
            if (!result.Succeeded)
            {
                var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Failed to delete user.";
                return StatusCode(500, new { message = errorMessage });
            }

            return Ok(new { message = "User deleted successfully." });
        }

        // GET /User/GetUploadStats/{id} (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetUploadStats(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Invalid user ID." });

            var (fileCount, totalSizeBytes, limitBytes) = await _userService.GetUserUploadStatsAsync(id);

            var usageMB = totalSizeBytes / (1024.0 * 1024.0);
            var limitMB = limitBytes.HasValue ? limitBytes.Value / (1024.0 * 1024.0) : (double?)null;
            var usagePercentage = limitBytes.HasValue ? (totalSizeBytes / (double)limitBytes.Value) * 100 : 0;

            return Ok(new
            {
                fileCount,
                totalSizeBytes,
                usageMB = Math.Round(usageMB, 2),
                limitMB = limitMB.HasValue ? Math.Round(limitMB.Value, 2) : (double?)null,
                usagePercentage = Math.Round(usagePercentage, 2),
                isUnlimited = !limitBytes.HasValue
            });
        }
    }

    public class ChangeDepartmentRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
    }

    public class SetUploadLimitRequest
    {
        public string UserId { get; set; } = string.Empty;
        public long? UploadLimitMB { get; set; }
    }

    public class DeactivateAccountRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ReactivateAccountRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class DeleteUserRequest
    {
        public string UserId { get; set; } = string.Empty;
    }
}
