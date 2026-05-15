using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Entities.File
{
    public class FileMetaData : BaseEntity
    {
        public long Size { get; set; }
       
        public long SizeKB
        {
            get
            {
                return Size / 1024;
            }

        }
        public long SizeMB
        {
            get
            {
                return SizeKB / 1024;
            }
        }

        public string? OrginalName { get; set; }
        public string? Description { get; set; }

        public Guid? LabelId { get; set; }
        public virtual Label Label { get; set; }

        public string StoredPath
        {
            get
            {
                return $"{CreatedBy}/{OrginalName}";
            }
        }

        public string FileUrl
        {
            get
            {

                return $"/{StoredPath.Replace("\\", "/")}";
            }
        }
    }
}
