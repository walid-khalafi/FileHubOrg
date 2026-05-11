using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.File;
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

        [HttpGet]
        public async Task<IActionResult> UserSharedFiles(string id)
        {


            var user = await _userService.GetUserProfileAsync(id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            var model = new UserSharedFiles()
            {
                files = new List<Domain.Entities.File.FileMetaData>(),
                userId = id,
                FullName = user.FullName,
                DepartmentId = user.DepartmentId.Value,
                DepartmentName = user.Department.Name

            };

            var files = await _fileService.GetFilesAsync(id);

            foreach (var file in files)
            {

                if (!file.CreatedBy.Equals(GetUserId()))
                {
                    var members = await _fileService.GetFileMembersAsync(id, file.Id);
                    var isCurrentUserIsMember = members.Any(x => x.AssignedToId.Equals(GetUserId()));
                    if (!isCurrentUserIsMember)
                    {
                        files.Remove(file);
                    }
                }
            }
            if (files.Any())
            {
                model.files = files;
            }
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file, Guid? labelId)
        {
            var userId = GetUserId();


            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "فایلی انتخاب نشده است.";
                return RedirectToAction("Index");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            var fileRecord = new FileMetaData
            {
                OrginalName = file.FileName,
                Size = file.Length,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedByIP = "",
                LabelId = labelId
            };

            try
            {
                using var stream = file.OpenReadStream();
                await _fileService.UploadFileAsync(fileRecord, stream);
                TempData["Success"] = "فایل با موفقیت آپلود شد.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch
            {
                TempData["Error"] = "خطایی در آپلود فایل رخ داد.";
            }

            return RedirectToAction("Index", "File", new { id = labelId });
        }


        [HttpPost]
        public async Task<IActionResult> GenerateDownloadToken([FromBody] Guid fileId)
        {
            var userId = GetUserId();

            var file = await _fileService.GetFileAsync(fileId);
            if (file ==null)
            {
                return NotFound("File NotFound!");
            }

            if (!file.CreatedBy.Equals(userId))
            {
                var members = await _fileService.GetFileMembersAsync(userId, fileId);
                var isCurrentUserIsMember = members.Any(x => x.AssignedToId.Equals(GetUserId()));
                if (!isCurrentUserIsMember)
                {
                    return Forbid();
                }
            }
           

            var jwt = await _jwtService.GenerateDownloadJwtAsync(fileId, userId);

            var downloadUrl = Url.Action("DownloadViaJwt", "File", new { token = jwt }, Request.Scheme);

            return Ok(new { downloadUrl });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadViaJwt(string token)
        {
            var principal = _jwtService.ValidateDownloadToken(token);
            if (principal == null)
                return Unauthorized();

            var fileId = Guid.Parse(principal.FindFirst("fileId")?.Value);

            var userId = principal.FindFirst("userId")?.Value;

            if (fileId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
                return BadRequest();


            var file = await _fileService.GetFileAsync(fileId);

           

            if (file == null || !System.IO.File.Exists(Path.Combine(_fileService.GetRootPath(), file.CreatedBy, file.OrginalName)))
                return NotFound();

            if (!file.CreatedBy.Equals(userId))
            {
                var members = await _fileService.GetFileMembersAsync(userId, fileId);
                var isCurrentUserIsMember = members.Any(x => x.AssignedToId.Equals(GetUserId()));
                if (!isCurrentUserIsMember)
                {
                    return Forbid();
                }
            }


            var dbToken = await _jwtService.GetDownloadTokenAsync(token);
            if (dbToken == null)
            {
                return NotFound();
            }

            dbToken.IsUsed = true;
            dbToken.UsedAt = DateTime.UtcNow;
            await _jwtService.UpdateDownloadToken(dbToken);
            var filePath = Path.Combine(_fileService.GetRootPath(), file.CreatedBy, file.OrginalName);

            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, file.OrginalName);

        }

    }
}
