using FileHubOrg.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FileHubOrg.Web.Models.DepartmentViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
namespace FileHubOrg.Web.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }
        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel() { Departments = new List<Domain.Entities.Organization.Department>() };
            IReadOnlyList<Domain.Entities.Organization.Department> departments = await _departmentService.GetDepartmentsAsync();

            if (departments !=null)
            {
                model.Departments = departments.ToList();
            }
          
            return View(model);
        }

        public async Task<IActionResult> Users(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var department = await _departmentService.GetDepartmentsByIdAsync(id);
            if (department == null)
                return NotFound();

            var users = await _departmentService.GetDepartmentUsersAsync(id);

            var model = new DepartmentUsersViewModel
            {
                DepartmentId   = id,
                DepartmentName = department.Name,
                Users          = users ?? new List<Domain.Entities.User.ApplicationUser>()
            };

            return View(model);
        }

        // ============================
        // Admin CRUD
        // ============================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new CreateDepartmentViewModel());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var createdBy = User?.Identity?.Name ?? string.Empty;
            var createdByIp = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

            await _departmentService.CreateDepartmentAsync(model.Name, createdBy, createdByIp);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var department = await _departmentService.GetDepartmentsByIdAsync(id);
            if (department == null)
                return NotFound();

            var model = new EditDepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditDepartmentViewModel model)
        {
            if (id == Guid.Empty)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View(model);
            }

            var updatedBy = User?.Identity?.Name ?? string.Empty;
            var updatedByIp = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

            var ok = await _departmentService.UpdateDepartmentAsync(id, model.Name, updatedBy, updatedByIp);
            if (!ok)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var ok = await _departmentService.DeleteDepartmentAsync(id);
            if (!ok)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
