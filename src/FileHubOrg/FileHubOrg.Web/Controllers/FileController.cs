using FileHubOrg.Application.Interfaces;
using FileHubOrg.Web.Models.FileViewModels;
using FileHubOrg.Web.Models.LabelViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileHubOrg.Web.Controllers
{
    [Authorize]
    public class FileController : Controller
    {

        private readonly IFileService _fileService;
        private readonly IApplicationUserService _userService;
        private readonly IJWTService _jwtService;
        private readonly ILabelService _labelService;

        public FileController(IFileService fileService,
                              IApplicationUserService userService,
                              IJWTService jwtService,
                              ILabelService labelService)
        {
            _fileService = fileService;
            _userService = userService;
            _jwtService = jwtService;
            _labelService = labelService;
        }
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index(Guid? id)
        {

            IndexViewModel model = new IndexViewModel()
            {
                files = new List<Domain.Entities.File.FileMetaData>()
            };

            List<Domain.Entities.File.FileMetaData> files = await _fileService.GetFilesAsync(GetUserId());

            if (id != null)
            {
                var label = await _labelService.GetLabelByIdAsync(id.Value);
                if (label != null)
                {
                    files = files.Where(x => x.LabelId == id).ToList();
                    model.labelName = label.Name;
                    model.labelId = id;
                }

            }
            model.files = files;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLabel(CreateLabelViewModel model)
        {
            if (ModelState.IsValid)
            {

                var isLabelCreated = await _labelService.CreateLabelAsync(GetUserId(), model.Name);
                if (isLabelCreated)
                {
                    TempData["success_msg"] = $"Label {model.Name} is created successfully.";
                }
                else
                {
                    TempData["error_msg"] = $"Have a problem to create label: {model.Name}";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
