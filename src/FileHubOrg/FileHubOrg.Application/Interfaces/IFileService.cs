using FileHubOrg.Domain.Entities.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Interfaces
{
    public interface IFileService
    {
        Task<List<FileMetaData>> GetFilesAsync(string userId);
        Task<FileMetaData> GetFileAsync(Guid fileId);
        Task<List<FileMember>> GetFileMembersAsync(string userId, Guid fileId);
        Task<bool> AddFileMembers(Guid fileId, List<string> userIds);
        Task<bool> RemoveFileMember(Guid fileId, string userId);
        Task<bool> DeleteFileAsync(Guid fileId, string userId);
        Task<FileMetaData> UploadFileAsync(FileMetaData file, Stream stream);
        Task<string> GenerateDownloadTokenAsync(Guid fileId, string userId);
        Task<bool> UpdateFileLabelAsync(Guid fileId, Guid? labelId, string userId);

        /// <summary>
        /// Persists file metadata to the database without copying any file data.
        /// Used by chunked upload after the temp file has already been moved to final storage.
        /// </summary>
        Task<FileMetaData> PersistFileMetaDataAsync(FileMetaData file);

        string GetRootPath();
    }
}
