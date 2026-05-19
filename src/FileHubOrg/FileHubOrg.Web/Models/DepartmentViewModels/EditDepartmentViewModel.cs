using System;
using System.ComponentModel.DataAnnotations;

namespace FileHubOrg.Web.Models.DepartmentViewModels
{
    public class EditDepartmentViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

