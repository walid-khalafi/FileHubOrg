using System;
using System.Collections.Generic;

namespace FileHubOrg.Web.Models.UserViewModels
{
    public class UserAuditLogViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public IReadOnlyList<AuditLogItemViewModel> UploadLogs { get; set; } = Array.Empty<AuditLogItemViewModel>();
        public IReadOnlyList<AuditLogItemViewModel> ShareLogs { get; set; } = Array.Empty<AuditLogItemViewModel>();
    }

    public class AuditLogItemViewModel
    {
        public DateTime? CreatedAt { get; set; }

        // Actor who performed the action (uploader/sharer)
        public string ActorUserId { get; set; } = string.Empty;
        public string ActorFullName { get; set; } = string.Empty;

        // IP address recorded at creation time
        public string IpAddress { get; set; } = "—";

        // Target
        public Guid? FileId { get; set; }
        public string FileName { get; set; } = string.Empty;

        // For share actions only
        public string? SharedToUserId { get; set; }
        public string? SharedToFullName { get; set; }
    }
}

