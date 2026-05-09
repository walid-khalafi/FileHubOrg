using FileHubOrg.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FileHubOrg.Web.Models.DepartmentViewModels;
namespace FileHubOrg.Web.Controllers
{

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

            model.Departments = departments.ToList();
            return View(model);
        }

        public IActionResult Users()
        {
            return View();
        }
    }
}
