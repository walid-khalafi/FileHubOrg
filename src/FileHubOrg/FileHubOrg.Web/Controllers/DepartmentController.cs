using FileHubOrg.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FileHubOrg.Web.Models.DepartmentViewModels;
using Microsoft.AspNetCore.Authorization;
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
            var model = new DepartmentUsersViewModel() { Users = new List<Domain.Entities.User.ApplicationUser>() };
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var users = await _departmentService.GetDepartmentUsersAsync(id);
            if (users !=null)
            {
                model.Users = users;
            }
            return View(model);
        }
    }
}
