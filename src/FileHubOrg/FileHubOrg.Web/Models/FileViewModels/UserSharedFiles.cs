using FileHubOrg.Domain.Entities.File;

namespace FileHubOrg.Web.Models.FileViewModels
{
    public class UserSharedFiles
    {
        public string userId { get; set; }
        public string FullName { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<FileMetaData> files { get; set; }
    }
}
