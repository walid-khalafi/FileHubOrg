using FileHubOrg.Domain.Entities.Organization;
using System.Collections.Generic;

namespace FileHubOrg.Web.Models.UserViewModels
{
    public class UserStorageReportViewModel
    {
        public IReadOnlyList<UserStorageReportRowViewModel> Rows { get; set; } = new List<UserStorageReportRowViewModel>();

        public IReadOnlyList<Department> Departments { get; set; } = new List<Department>();
    }
}

