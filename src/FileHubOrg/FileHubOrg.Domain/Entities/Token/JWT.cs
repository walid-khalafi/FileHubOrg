using FileHubOrg.Domain.Entities.File;
using FileHubOrg.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Entities.Token
{
    public class JWT :BaseEntity
    {
        public Guid FileMetadataId { get; set; }
        public virtual FileMetaData FileMetaData { get; set; }

        public string RequestedByUserId { get; set; } = null!;
        public ApplicationUser RequestedByUser { get; set; } = null!;
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }

        public string Jwt { get; set; } = string.Empty;
    }
}
