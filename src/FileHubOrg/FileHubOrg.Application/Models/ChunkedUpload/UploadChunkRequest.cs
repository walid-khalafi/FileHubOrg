using System;
using Microsoft.AspNetCore.Http;

namespace FileHubOrg.Application.Models.ChunkedUpload
{
    public class UploadChunkRequest
    {
        public Guid UploadId { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public long ChunkSizeBytes { get; set; }
        public IFormFile ChunkFile { get; set; } = default!;
        public long ExpectedTotalSizeBytes { get; set; }
        public Guid? LabelId { get; set; }
    }
}

