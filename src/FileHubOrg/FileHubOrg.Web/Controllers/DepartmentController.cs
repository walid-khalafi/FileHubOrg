using Microsoft.AspNetCore.Mvc;

namespace FileHubOrg.Web.Controllers
{
    public class DepartmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }
    }
}
