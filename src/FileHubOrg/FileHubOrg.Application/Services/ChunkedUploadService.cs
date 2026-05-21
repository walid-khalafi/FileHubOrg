using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using FileHubOrg.Application.Configurations;
using FileHubOrg.Application.Interfaces;
using FileHubOrg.Application.Models.ChunkedUpload;
using FileHubOrg.Domain.Entities.File;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace FileHubOrg.Application.Services
{
    public class ChunkedUploadService : IChunkedUploadService
    {
        private readonly IChunkedUploadSessionStore _store;
        private readonly IFileService _fileService;
        private readonly FileStorageOptions _options;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChunkedUploadService(
            IChunkedUploadSessionStore store,
            IFileService fileService,
            IOptions<FileStorageOptions> options,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _store = store;
            _fileService = fileService;
            _options = options.Value;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<Guid> InitAsync(string originalFileName, long totalSizeBytes, Guid? labelId, int totalChunks, long chunkSizeBytes)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            if (totalSizeBytes <= 0)
                throw new InvalidOperationException("Invalid file size");

            // validate size & extension similar to UploadFileAsync
            var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("File extension not allowed.");

            var maxSizeBytes = _options.MaxChunkedFileSizeMB > 0
                ? _options.MaxChunkedFileSizeMB * 1024L * 1024L
                : 2048L * 1024L * 1024L;   // 2 GB fallback
            if (totalSizeBytes > maxSizeBytes)
                throw new InvalidOperationException("File too large.");

            if (totalChunks <= 0 || chunkSizeBytes <= 0)
                throw new InvalidOperationException("Invalid chunking");

            var root = _fileService.GetRootPath();
            var tempRoot = Path.Combine(root, ".uploads-temp", userId);
            Directory.CreateDirectory(tempRoot);

            var tempFilePath = Path.Combine(tempRoot, Guid.NewGuid().ToString("N") + ".part");

            var session = new UploadSession
            {
                UploadId = Guid.NewGuid(),
                CreatedBy = userId,
                OriginalFileName = Path.GetFileName(originalFileName),
                TotalSizeBytes = totalSizeBytes,
                LabelId = labelId,
                TempFilePath = tempFilePath,
                TotalChunks = totalChunks,
                ChunkSizeBytes = chunkSizeBytes,
                ReceivedBytes = 0,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _store.CreateSessionAsync(session);
            return session.UploadId;
        }

        public async Task<bool> UploadChunkAsync(Guid uploadId, int chunkIndex, int totalChunks, IFormFile chunk)
        {
            var userId = GetUserId();
            var session = await _store.GetSessionAsync(uploadId);
            if (session == null) return false;
            if (!string.Equals(session.CreatedBy, userId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException();

            if (chunkIndex < 0 || chunkIndex >= session.TotalChunks)
                throw new InvalidOperationException("Invalid chunk index");

            if (totalChunks != session.TotalChunks)
                throw new InvalidOperationException("Invalid totalChunks");

            var chunkStream = chunk.OpenReadStream();

            // Write chunk at offset.
            var offset = chunkIndex * session.ChunkSizeBytes;

            Directory.CreateDirectory(Path.GetDirectoryName(session.TempFilePath)!);

            // Ensure temp file exists with correct length before writing.
            // Race/ordering issues can cause the file to be missing when a non-first chunk arrives.
            if (!File.Exists(session.TempFilePath))
            {
                using var fsInit = new FileStream(session.TempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                fsInit.SetLength(session.TotalSizeBytes);
            }
            else if (chunkIndex == 0)
            {
                // If first chunk, (re)ensure length.
                using var fsInit = new FileStream(session.TempFilePath, FileMode.Open, FileAccess.Write, FileShare.Read);
                fsInit.SetLength(session.TotalSizeBytes);
            }

            using var fs = new FileStream(session.TempFilePath, FileMode.Open, FileAccess.Write, FileShare.Read);
            fs.Seek(offset, SeekOrigin.Begin);
            await chunkStream.CopyToAsync(fs);


            // update received bytes best-effort (no per-chunk bitmap)
            var uploadedSoFar = offset + chunk.Length;
            if (uploadedSoFar > session.ReceivedBytes)
                session.ReceivedBytes = uploadedSoFar;

            await _store.UpdateSessionAsync(session);
            return true;
        }

        public async Task<FileHubOrg.Application.Models.ChunkedUpload.UploadStatusResponse> GetStatusAsync(Guid uploadId)
        {
            var userId = GetUserId();
            var session = await _store.GetSessionAsync(uploadId);
            if (session == null)
                throw new InvalidOperationException("Upload session not found");

            if (!string.Equals(session.CreatedBy, userId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException();

            var totalChunks = session.TotalChunks;
            var chunkSize = session.ChunkSizeBytes;

            // Next chunk index derived from received bytes.
            var nextChunk = (int)Math.Floor(session.ReceivedBytes / (double)chunkSize);
            if (nextChunk < 0) nextChunk = 0;
            if (nextChunk > totalChunks) nextChunk = totalChunks;

            var percent = session.TotalSizeBytes <= 0
                ? 0
                : (int)Math.Round((session.ReceivedBytes / (double)session.TotalSizeBytes) * 100);
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            return new FileHubOrg.Application.Models.ChunkedUpload.UploadStatusResponse
            {
                UploadId = uploadId,
                TotalSizeBytes = session.TotalSizeBytes,
                ReceivedBytes = session.ReceivedBytes,
                TotalChunks = session.TotalChunks,
                ChunkSizeBytes = session.ChunkSizeBytes,
                NextChunkIndex = nextChunk,
                Percent = percent
            };
        }

        public async Task<Guid> CompleteAsync(Guid uploadId)
        {

            var userId = GetUserId();
            var session = await _store.GetSessionAsync(uploadId);
            if (session == null) throw new InvalidOperationException("Upload session not found");
            if (!string.Equals(session.CreatedBy, userId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException();

            // Final validations
            var fileSize = new FileInfo(session.TempFilePath).Length;
            if (fileSize != session.TotalSizeBytes)
                throw new InvalidOperationException("Upload size mismatch");

            var fileRecord = new FileMetaData
            {
                OrginalName = session.OriginalFileName,
                Size = session.TotalSizeBytes,
                CreatedBy = session.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                CreatedByIP = "",
                LabelId = session.LabelId
            };

            // Move assembled temp file to final storage location directly —
            // bypasses UploadFileAsync's MaxFileSizeMB check (already validated at InitAsync).
            var uploaderFolder = Path.Combine(_fileService.GetRootPath(), fileRecord.CreatedBy);
            Directory.CreateDirectory(uploaderFolder);

            var targetPath = Path.Combine(uploaderFolder, Path.GetFileName(fileRecord.OrginalName));

            if (File.Exists(targetPath))
                File.Delete(targetPath);

            File.Move(session.TempFilePath, targetPath);

            // Persist metadata to DB (no file copy, no size re-check).
            await _fileService.PersistFileMetaDataAsync(fileRecord);


            await _store.DeleteSessionAsync(uploadId);
            return fileRecord.Id;
        }
    }
}

