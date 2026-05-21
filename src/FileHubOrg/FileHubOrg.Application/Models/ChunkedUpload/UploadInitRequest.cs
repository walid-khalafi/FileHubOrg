using System;
using Microsoft.AspNetCore.Http;

namespace FileHubOrg.Application.Models.ChunkedUpload
{
    public class UploadInitRequest
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public long TotalSizeBytes { get; set; }
        public Guid? LabelId { get; set; }
        public int TotalChunks { get; set; }
        public long ChunkSizeBytes { get; set; }
        public IFormFile? ClientProvidedFile { get; set; }
    }
}

