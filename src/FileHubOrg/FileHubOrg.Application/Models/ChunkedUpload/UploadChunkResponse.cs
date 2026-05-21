namespace FileHubOrg.Application.Models.ChunkedUpload
{
    public class UploadChunkResponse
    {
        public long ReceivedBytes { get; set; }
        public bool Ok { get; set; }
    }
}

