using System;
using System.Threading.Tasks;
using FileHubOrg.Application.Models.ChunkedUpload;

namespace FileHubOrg.Application.Interfaces
{
    public interface IChunkedUploadSessionStore
    {
        Task<UploadSession> CreateSessionAsync(UploadSession session);
        Task<UploadSession?> GetSessionAsync(Guid uploadId);
        Task UpdateSessionAsync(UploadSession session);
        Task DeleteSessionAsync(Guid uploadId);
    }
}

