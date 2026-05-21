using System;

namespace FileHubOrg.Application.Models.ChunkedUpload
{
    public class UploadInitResponse
    {
        public Guid UploadId { get; set; }
        public int TotalChunks { get; set; }
        public long ChunkSizeBytes { get; set; }
        public string TempFilePath { get; set; } = string.Empty;
    }
}

