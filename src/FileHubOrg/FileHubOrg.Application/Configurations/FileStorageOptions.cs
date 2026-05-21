using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Configurations
{
    public class FileStorageOptions
    {
        public string StorageMode { get; set; }
        public StoragePathSettings Local { get; set; }
        public StoragePathSettings Network { get; set; }
        public List<string> AllowedExtensions { get; set; } = new();
        public int MaxFileSizeMB { get; set; }

        /// <summary>
        /// Maximum file size in MB for chunked (resumable) uploads.
        /// Defaults to 2048 MB (2 GB) if not configured.
        /// </summary>
        public int MaxChunkedFileSizeMB { get; set; } = 2048;
    }
}
