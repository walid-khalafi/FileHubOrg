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



        public Task<bool> DeleteFileAsync(Guid fileId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateDownloadTokenAsync(Guid fileId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<FileMember>> GetFileMembersAsync(string userId, Guid fileId)
        {
            var file = await _unitOfWork.FileMetaData.GetFirstOrDefaultAsync(x => x.Id == fileId && x.CreatedBy == userId);
            if (file == null)
            {
                throw new Exception("File Not Found");
            }

            List<FileMember> members = await _unitOfWork.FileMembers
                .AsQueryable(x => x.FileMetadataId == fileId)
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

            await _unitOfWork.FileMetaData.AddAsync(file);
            await _unitOfWork.SaveChangesAsync();

            return file;
        }
    }
}
