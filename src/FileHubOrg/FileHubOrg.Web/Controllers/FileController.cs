using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileHubOrg.Web.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
