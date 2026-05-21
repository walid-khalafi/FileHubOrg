using System;

namespace FileHubOrg.Application.Models.ChunkedUpload
{
    public class UploadStatusResponse
    {
        public Guid UploadId { get; set; }
        public long TotalSizeBytes { get; set; }
        public long ReceivedBytes { get; set; }

        public int TotalChunks { get; set; }
        public int NextChunkIndex { get; set; }

        public long ChunkSizeBytes { get; set; }

        public int Percent { get; set; }
    }
}

