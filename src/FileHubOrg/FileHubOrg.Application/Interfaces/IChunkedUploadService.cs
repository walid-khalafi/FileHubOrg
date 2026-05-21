using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FileHubOrg.Application.Interfaces
{
    public interface IChunkedUploadService
    {
        Task<Guid> InitAsync(string originalFileName, long totalSizeBytes, Guid? labelId, int totalChunks, long chunkSizeBytes);
        Task<bool> UploadChunkAsync(Guid uploadId, int chunkIndex, int totalChunks, IFormFile chunk);
        Task<FileHubOrg.Application.Models.ChunkedUpload.UploadStatusResponse> GetStatusAsync(Guid uploadId);

        Task<Guid> CompleteAsync(Guid uploadId);

    }
}

