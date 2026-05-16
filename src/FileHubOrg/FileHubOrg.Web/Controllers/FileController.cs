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
        private readonly IDepartmentService _departmentService;

        public FileController(IFileService fileService,
                              IApplicationUserService userService,
                              IJWTService jwtService,
                              ILabelService labelService,IDepartmentService departmentService)
        {
            _fileService = fileService;
            _userService = userService;
            _jwtService = jwtService;
            _labelService = labelService;
            _departmentService = departmentService;
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


        [HttpGet]
        public async Task<IActionResult> GetShareList(Guid fileId)
        {
            // 1. --- Input Validation ---
            // Check if the provided fileId is empty (or default Guid).
            // An empty fileId might indicate an invalid request.
            if (fileId == Guid.Empty)
            {
                // Log a warning if needed (assuming _logger is available via DI)
                // _logger.LogWarning("GetShareList called with an empty FileId.");

                // Return a BadRequest response, as the request is invalid.
                // It's better to return BadRequest than a PartialView with an empty list in this case.
                return BadRequest("Invalid File ID provided.");
            }

            // 2. --- Data Retrieval ---
            try
            {
                // Fetch all departments from the department service.
                // Use null-conditional operator ?. and null-coalescing operator ?? for safety.
                // This ensures that if _departmentService.GetDepartmentsAsync() returns null,
                // we get an empty list instead of a NullReferenceException.
                var departments = await _departmentService.GetDepartmentsAsync() ?? new List<Domain.Entities.Organization.Department>();

                // Exclude the current user from the share picker list.
                var currentUserId = GetUserId();
                var visibleDepartments = departments
                    .Select(d =>
                    {
                        d.Members = d.Members?
                            .Where(u => !string.Equals(u.Id, currentUserId, StringComparison.OrdinalIgnoreCase))
                            .ToList() ?? new List<Domain.Entities.User.ApplicationUser>();
                        return d;
                    })
                    .Where(d => d.Members != null && d.Members.Any())
                    .ToList();

                // Prepare the model for the Partial View (_DepartmentList).
                var model = new ShareListViewModel
                {
                    fileId = fileId, // Pass the valid fileId to the model.
                    Departments = visibleDepartments,
                };

                // 3. --- Response ---
                // Return the Partial View with the populated model.
                // The view "_DepartmentList" is responsible for rendering the department and user list UI.
                return PartialView("_DepartmentList", model);
            }
            catch (Exception ex)
            {
                // --- Exception Handling ---
                // Log the exception details for troubleshooting.
                // Using Console.WriteLine is not ideal for production; use ILogger instead.
                // _logger.LogError(ex, "An error occurred while retrieving departments for sharing file {FileId}.", fileId);
                Console.WriteLine($"Error in GetShareList for FileId {fileId}: {ex.Message}"); // Keeping original Console.WriteLine as requested.

                // Return a Partial View containing an error message to maintain the modal structure.
                // This informs the user that the data could not be loaded.
                // A dedicated error partial view or just HTML string can be returned.
                return PartialView("_DepartmentList", new ShareListViewModel
                {
                    fileId = fileId,
                    Departments = new List<Domain.Entities.Organization.Department>(), // Ensure Departments list is empty on error
                    ErrorMessage = "Error loading departments. Please try again later." // Example property to pass error message
                });
            }
        }

        /// <summary>Returns the current member list for a file (owner only).</summary>
        [HttpGet]
        public async Task<IActionResult> GetFileMembers(Guid fileId)
        {
            if (fileId == Guid.Empty) return BadRequest();

            var file = await _fileService.GetFileAsync(fileId);
            if (file == null) return NotFound();

            var members = await _fileService.GetFileMembersAsync(GetUserId(), fileId);
            var currentUserId = GetUserId();
            if (!string.Equals(file.CreatedBy, currentUserId, StringComparison.Ordinal) &&
                !members.Any(x => string.Equals(x.AssignedToId, currentUserId, StringComparison.Ordinal)))
            {
                return Forbid();
            }

            var result = members.Select(m => new
            {
                userId   = m.AssignedToId,
                fullName = m.AssignedTo?.FullName ?? m.AssignedToId,
                email    = m.AssignedTo?.Email ?? string.Empty,
                addedAt  = m.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Ok(result);
        }

        /// <summary>Removes a single member from a file (owner only).</summary>
        [HttpPost]
        public async Task<IActionResult> RemoveFileMember([FromBody] RemoveFileMemberRequest request)
        {
            if (request == null || request.FileId == Guid.Empty || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest();

            var file = await _fileService.GetFileAsync(request.FileId);
            if (file == null) return NotFound();
            if (!file.CreatedBy.Equals(GetUserId())) return Forbid();

            var success = await _fileService.RemoveFileMember(request.FileId, request.UserId);
            if (!success)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to remove member." });

            return Ok(new { message = "Member removed." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile([FromBody] Guid fileId)
        {
            if (fileId == Guid.Empty)
                return BadRequest("Invalid file ID.");

            var userId = GetUserId();
            var file = await _fileService.GetFileAsync(fileId);

            if (file == null)
                return NotFound();

            if (!file.CreatedBy.Equals(userId))
                return Forbid();

            var success = await _fileService.DeleteFileAsync(fileId, userId);
            if (!success)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete file." });

            return Ok(new { message = "File deleted." });
        }

        [HttpPost]
        public async Task<IActionResult> ShareFile([FromBody] ShareFileRequestModel request)
        {
            // 1. --- Input Validation ---
            // Check if the request body is null.
            if (request == null)
            {
                // Log the warning if needed (assuming _logger is available via DI)
                // _logger.LogWarning("ShareFile request received with null body.");
                return BadRequest("Invalid request data: Request body is null.");
            }

            // Check if the model state is valid. This relies on data annotations on the ShareFileRequestModel.
            if (!ModelState.IsValid)
            {
                // Log the validation errors if needed
                // _logger.LogWarning("ShareFile request received with invalid model state. Errors: {ModelStateErrors}", ModelState);
                return BadRequest(ModelState); // Return validation errors to the client
            }

            // Further validation: Ensure FileId is not empty and at least one user ID is provided.
            if (request.FileId == Guid.Empty)
            {
                // _logger.LogWarning("ShareFile request received with an invalid (empty) FileId.");
                return BadRequest("Invalid File ID provided.");
            }

            // Check if the SharedToUserIds array is null or empty.
            // Using 'request.SharedToUserIds?.Any() ?? false' is a safer way to check.
            if (!(request.SharedToUserIds?.Any() ?? false))
            {
                // _logger.LogWarning("ShareFile request for FileId {FileId} received with no users selected.", request.FileId);
                return BadRequest("No users selected for sharing.");
            }

            // 2. --- Business Logic Execution ---
            try
            {
                // Convert the array of user IDs to a List<string> for the service method.
                List<string> userIds = request.SharedToUserIds.ToList();
                Guid fileGuid = request.FileId;

                // Call the service method to add members to the file.
                // Assuming _fileService is injected and has AddFileMembers method.
                bool success = await _fileService.AddFileMembers(fileGuid, userIds);

                // 3. --- Response Handling ---
                if (success)
                {
                    // Log successful operation if logger is available
                    // _logger.LogInformation("File {FileId} shared successfully with {UserCount} users.", fileGuid, userIds.Count);

                    // Return a success response (HTTP 200 OK) with a confirmation message.
                    return Ok(new
                    {
                        message = "File shared successfully.",
                        fileId = fileGuid,
                        sharedWithCount = userIds.Count
                    });
                }
                else
                {
                    // Log failure if the service returned false (e.g., file not found, permission issues).
                    // _logger.LogError("Failed to share file {FileId}. The AddFileMembers service returned false.", fileGuid);

                    // Return an Internal Server Error (HTTP 500) indicating a failure in the process.
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while sharing the file. Service operation failed." });
                }
            }
            catch (Exception ex)
            {
                // --- Exception Handling ---
                // Log the exception details for troubleshooting.
                // Using Console.WriteLine is not ideal for production; use ILogger instead.
                // _logger.LogError(ex, "An unexpected error occurred during file sharing for FileId {FileId}.", request.FileId);
                Console.WriteLine($"Error in ShareFile for FileId {request.FileId}: {ex.Message}"); // Keeping original Console.WriteLine as requested not to add new code, but it's not best practice.

                // Return an Internal Server Error (HTTP 500) to the client.
                // Avoid exposing detailed exception messages directly to the client.
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
    }

    public class RemoveFileMemberRequest
    {
        public Guid FileId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
