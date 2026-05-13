using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;

namespace FileHubOrg.Web.Models.FileViewModels
{
    public class ShareListViewModel
    {
        public Guid fileId { get; set; }
        public List<Department>  Departments { get; set; }
    }
}
