using FileHubOrg.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Entities.File
{
    public class FileMember:BaseEntity
    {

        public Guid FileMetadataId { get; set; }
        public string AssignedToId { get; set; } = string.Empty;
        public virtual FileMetaData FileMetaData {  get; set; }
        public virtual ApplicationUser AssignedTo {  get; set; }
    }
}
