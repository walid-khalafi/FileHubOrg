using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Entities.File
{
    public class Label:BaseEntity
    {
        public string Name { get; set; }
        public List<FileMetaData> Files { get; set; }
    }
}
