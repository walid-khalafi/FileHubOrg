using FileHubOrg.Application.Interfaces;
using FileHubOrg.Web.Models.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileHubOrg.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IApplicationUserService _userService;
        private readonly IDepartmentService _departmentService;

        public UserController(IApplicationUserService userService, IDepartmentService departmentService)
        {
            _userService = userService;
            _departmentService = departmentService;
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
}
