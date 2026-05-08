using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Entities.File
{
    public class FileMetaData:BaseEntity
    {
        public long Size { get; set; }
        public string? OrginalName { get; set; }
        public string? Description { get; set; }

        public Guid? LabelId { get; set; }
        public virtual Label Label { get; set; }
    }
}
