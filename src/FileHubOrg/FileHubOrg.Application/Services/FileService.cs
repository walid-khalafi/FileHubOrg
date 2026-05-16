using FileHubOrg.Application.Configurations;
using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.File;
using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FileStorageOptions _options;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;



        public FileService(IUnitOfWork unitOfWork, IOptions<FileStorageOptions> options, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _options = options.Value;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        public async Task<bool> AddFileMembers(Guid fileId, List<string> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                // اگر لیستی از کاربران داده نشده، کاری انجام نمی‌دهیم
                return false;
            }

            // دریافت اطلاعات فایل. فرض می‌کنیم GetFileAsync در سرویس دیگری (IFileService) است.
            var file = await GetFileAsync(fileId);

            if (file == null)
            {
                // اگر فایل پیدا نشد، عملیات ناموفق است
                // می‌توانید یک exception مناسب هم اینجا پرتاب کنید
                return false;
            }

            // گرفتن IP Address کاربر فعلی
            // استفاده از ?. برای جلوگیری از NullReferenceException اگر HttpContext یا Connection null باشند
            string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var uniqueUserIds = userIds
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueUserIds.Any())
            {
                return false;
            }

            var existingMemberIds = await _unitOfWork.FileMembers
                .AsQueryable()
                .Where(x => x.FileMetadataId == fileId)
                .Select(x => x.AssignedToId)
                .ToListAsync();

            var newMemberIds = uniqueUserIds
                .Where(id => !existingMemberIds.Contains(id, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (!newMemberIds.Any())
            {
                return true;
            }

            // ساخت لیست FileMember با استفاده از LINQ برای خوانایی بهتر
            var newFileMembers = newMemberIds.Select(userId => new FileMember
            {
                Id = Guid.NewGuid(),
                FileMetadataId = fileId,
                AssignedToId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = file.CreatedBy, // شناسه کاربری که فایل را ایجاد کرده
                CreatedByIP = ipAddress // IP آدرس کاربر فعلی
            }).ToList(); // تبدیل نتیجه LINQ به لیست


            try
            {
                // اضافه کردن اعضای جدید به Unit of Work
                await _unitOfWork.FileMembers.AddRangeAsync(newFileMembers);

                // ذخیره تغییرات در دیتابیس
                await _unitOfWork.SaveChangesAsync();

                return true; // موفقیت آمیز بود
            }
            catch (Exception ex)
            {
                // در اینجا لاگ خطا را انجام دهید (مثلا با استفاده از ILogger)
                // مثال ساده:
                Console.WriteLine($"Error adding file members for file {fileId}: {ex.Message}");

                // بسته به سیاست برنامه، می‌توانید exception را دوباره پرتاب کنید یا false برگردانید
                // throw; // پرتاب مجدد exception برای مدیریت در لایه‌های بالاتر
                return false; // برگرداندن false نشان‌دهنده عدم موفقیت است
            }
        }

        public Task<bool> DeleteFileAsync(Guid fileId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveFileMember(Guid fileId, string userId)
        {
            try
            {
                var member = await _unitOfWork.FileMembers
                    .GetFirstOrDefaultAsync(x => x.FileMetadataId == fileId && x.AssignedToId == userId);

                if (member == null) return false;

                await _unitOfWork.FileMembers.RemoveAsync(member);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing file member: {ex.Message}");
                return false;
            }
        }

        public Task<string> GenerateDownloadTokenAsync(Guid fileId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<FileMetaData> GetFileAsync(Guid fileId) => await _unitOfWork.FileMetaData.GetFirstOrDefaultAsync(x => x.Id.Equals(fileId));

        public async Task<List<FileMember>> GetFileMembersAsync(string userId, Guid fileId)
        {
            var file = await _unitOfWork.FileMetaData.GetFirstOrDefaultAsync(x => x.Id == fileId);
            if (file == null)
            {
                throw new Exception("File Not Found");
            }

            List<FileMember> members = await _unitOfWork.FileMembers
                .AsQueryable()
                .Where(x => x.FileMetadataId == fileId)
                .Include(x => x.AssignedTo)
                .Include(f => f.FileMetaData)
                .ToListAsync();

            return members;
        }

        public async Task<List<FileMetaData>> GetFilesAsync(string userId)
        {

            var files = await _unitOfWork.FileMetaData.GetFilesByUserIdAsync(userId);
            if (files != null)
            {
                return files;
            }
            return new List<FileMetaData>();
        }

        public string GetRootPath()
        {
            return _options.StorageMode == "Network"
                ? _options.Network.PhysicalPath
                : _options.Local.PhysicalPath;
        }

        public async Task<FileMetaData> UploadFileAsync(FileMetaData file, Stream stream)
        {
            var maxSizeBytes = _options.MaxFileSizeMB * 1024 * 1024;

            if (file.Size > maxSizeBytes)
                throw new InvalidOperationException("حجم فایل از حد مجاز بیشتر است.");


            var ext = Path.GetExtension(file.OrginalName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("نوع فایل مجاز نیست.");

            var uploaderFolder = Path.Combine(GetRootPath(), file.CreatedBy);

            Directory.CreateDirectory(uploaderFolder);

            var fileName = Path.GetFileName(file.OrginalName);
            var fullPath = Path.Combine(uploaderFolder, fileName);

            using var fs = new FileStream(fullPath, FileMode.Create);
            await stream.CopyToAsync(fs);

            string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            file.CreatedByIP = ipAddress;
            await _unitOfWork.FileMetaData.AddAsync(file);
            await _unitOfWork.SaveChangesAsync();

            return file;
        }

        public async Task<bool> UpdateFileLabelAsync(Guid fileId, Guid? labelId, string userId)
        {
            try
            {
                var file = await _unitOfWork.FileMetaData.GetFirstOrDefaultAsync(x => x.Id == fileId);
                if (file == null)
                    return false;

                // Verify the user owns this file
                if (file.CreatedBy != userId)
                    return false;

                // If labelId is provided, verify it belongs to the user
                if (labelId.HasValue)
                {
                    var label = await _unitOfWork.Labels.GetFirstOrDefaultAsync(x => x.Id == labelId && x.CreatedBy == userId);
                    if (label == null)
                        return false;
                }

                file.LabelId = labelId;
                await _unitOfWork.FileMetaData.UpdateAsync(file);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating file label: {ex.Message}");
                return false;
            }
        }
    }
}
