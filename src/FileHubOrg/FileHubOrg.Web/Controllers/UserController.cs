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

            var model = new EditUserViewModel
            {
                UserId               = user.Id,
                FirstName            = user.FirstName,
                LastName             = user.LastName,
                Email                = user.Email ?? string.Empty,
                PhoneNumber          = user.PhoneNumber ?? string.Empty,
                DepartmentId         = user.DepartmentId,
                CurrentDepartmentName = user.Department?.Name,
                Departments          = departments
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
    }

    public class ChangeDepartmentRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
    }
}
