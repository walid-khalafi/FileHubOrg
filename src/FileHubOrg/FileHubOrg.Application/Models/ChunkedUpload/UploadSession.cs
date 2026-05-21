using System;

namespace FileHubOrg.Application.Models.ChunkedUpload
{
    public class UploadSession
    {
        public Guid UploadId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long TotalSizeBytes { get; set; }
        public Guid? LabelId { get; set; }

        // Temp file path where chunks are assembled.
        public string TempFilePath { get; set; } = string.Empty;

        public int TotalChunks { get; set; }
        public long ChunkSizeBytes { get; set; }

        // Server side received bytes tracking.
        public long ReceivedBytes { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Simple integrity check (optional).
        public string? ExpectedSha256Hex { get; set; }
    }
}

