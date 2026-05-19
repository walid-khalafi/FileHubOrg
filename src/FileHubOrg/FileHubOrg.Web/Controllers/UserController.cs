using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Entities.File;

using FileHubOrg.Web.Models.AccountViewModels;
using FileHubOrg.Web.Models.UserViewModels;
using Microsoft.EntityFrameworkCore;





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
        private readonly IFileService _fileService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IApplicationUserService userService,
            IDepartmentService departmentService,
            IFileService fileService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _departmentService = departmentService;
            _fileService = fileService;
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

        // GET /User/StorageReport
        [HttpGet]
        public async Task<IActionResult> StorageReport()
        {
            var users = await _userService.GetAllUsersAsync();
            var departments = await _departmentService.GetDepartmentsAsync();

            var departmentById = departments.ToDictionary(d => d.Id, d => d.Name);

            var rows = new System.Collections.Generic.List<UserStorageReportRowViewModel>();

            foreach (var user in users)
            {
                var (fileCount, totalSizeBytes, limitBytes) = await _userService.GetUserUploadStatsAsync(user.Id);

                var isUnlimited = !limitBytes.HasValue || limitBytes.Value <= 0;
                var usagePercentage = 0d;

                if (!isUnlimited)
                {
                    usagePercentage = (totalSizeBytes / (double)limitBytes!.Value) * 100;
                }

                var usedMb = totalSizeBytes / (1024.0 * 1024.0);
                double? limitMb = null;
                if (!isUnlimited)
                    limitMb = limitBytes!.Value / (1024.0 * 1024.0);

                departmentById.TryGetValue(user.DepartmentId ?? Guid.Empty, out var deptName);
                deptName = string.IsNullOrWhiteSpace(deptName) ? "—" : deptName;

                rows.Add(new UserStorageReportRowViewModel
                {
                    User = user,
                    DepartmentName = deptName,
                    IsUnlimited = isUnlimited,
                    UsagePercentage = usagePercentage,
                    UsedMb = usedMb,
                    LimitMb = limitMb
                });
            }

            return View(new UserStorageReportViewModel
            {
                Rows = rows,
                Departments = departments
            });
        }


        // GET /User/Activity/{id}
        public async Task<IActionResult> Activity(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user = await _userService.GetUserProfileAsync(id);
            if (user == null)
                return NotFound();

            var (fileCount, totalSizeBytes, limitBytes) = await _userService.GetUserUploadStatsAsync(id);

            var departmentName = user.Department?.Name ?? "—";

            // If upload limit is unlimited, UploadUsagePercentage cannot be computed.
            // Existing view model expects a percentage value; we show 0 in that case.
            var uploadUsagePercentage = 0d;
            if (limitBytes.HasValue && limitBytes.Value > 0)
            {
                uploadUsagePercentage = (totalSizeBytes / (double)limitBytes.Value) * 100;
            }

            var model = new UserActivityViewModel
            {
                UserId                   = user.Id,
                FullName                = user.FullName,
                Email                    = user.Email ?? string.Empty,
                PhoneNumber              = user.PhoneNumber ?? string.Empty,
                DepartmentName          = departmentName,
                IsActive                 = user.IsActive,
                LastLoginAt              = user.LastLoginAt,
                LastActivityAt           = user.LastActivityAt,
                UploadedFilesCount       = fileCount,
                TotalUploadedSizeMB      = totalSizeBytes / (1024.0 * 1024.0),
                UploadUsagePercentage    = uploadUsagePercentage
            };

            return View(model);
        }

        // GET /User/Audit/{id}
        [HttpGet]
        public async Task<IActionResult> Audit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user = await _userService.GetUserProfileAsync(id);
            if (user == null)
                return NotFound();

            // Upload logs: files created by this user.
            var uploads = await _fileService.GetFilesAsync(user.Id);

            var uploadLogs = (uploads ?? new List<FileMetaData>())
                .Select(f => new AuditLogItemViewModel
                {
                    CreatedAt = f.CreatedAt,
                    ActorUserId = f.CreatedBy,
                    ActorFullName = (f.CreatedBy == null || f.CreatedBy == "") ? "—" : (f.CreatedBy == user.Id ? user.FullName : user.FullName),
                    IpAddress = string.IsNullOrWhiteSpace(f.CreatedByIP) ? "—" : f.CreatedByIP,
                    FileId = f.Id,
                    FileName = f.OrginalName ?? "(no name)"
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            // Share logs: show files where this user is the assigned member (shared-to).
            // Important: GetFilesAsync(userId) returns ONLY files created/uploaded by userId,
            // so using it as share candidates may result in empty logs.
            // We instead iterate across files returned by GetFileMembersAsync for each candidate file.
            // To avoid needing a new service method, we query members per uploaded file for the user and also per shared file.
            // Since the service contract does not provide “files shared to user”, the correct way is to gather files from FileMembers.
            // Minimal fix: iterate all files uploaded by the actor for this audit page and pick members assigned to the audited user.

            var shareLogs = new List<AuditLogItemViewModel>();

            // Candidate files: files created by this user (existing behavior for actor uploads)
            var shareCandidates = await _fileService.GetFilesAsync(user.Id);
            foreach (var file in shareCandidates ?? new List<FileMetaData>())
            {
                var members = await _fileService.GetFileMembersAsync(user.Id, file.Id);
                foreach (var m in members)
                {
                    // Filter to “shared-to” entries for the audited user
                    if (!string.Equals(m.AssignedToId, user.Id, StringComparison.OrdinalIgnoreCase))
                        continue;

                    shareLogs.Add(new AuditLogItemViewModel
                    {
                        CreatedAt = m.CreatedAt,
                        ActorUserId = m.CreatedBy,
                        ActorFullName = user.FullName,


                        IpAddress = string.IsNullOrWhiteSpace(m.CreatedByIP) ? "—" : m.CreatedByIP,
                        FileId = file.Id,
                        FileName = file.OrginalName ?? "(no name)",
                        SharedToUserId = m.AssignedToId,
                        SharedToFullName = m.AssignedTo?.FullName ?? m.AssignedToId
                    });
                }
            }


            return View(new UserAuditLogViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                UploadLogs = uploadLogs,
                ShareLogs = shareLogs.OrderByDescending(x => x.CreatedAt).ToList()
            });
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
                LastLoginAt              = user.LastLoginAt,
                LastActivityAt           = user.LastActivityAt,
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
