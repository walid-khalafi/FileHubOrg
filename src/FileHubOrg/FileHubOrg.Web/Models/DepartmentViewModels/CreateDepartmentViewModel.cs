using System;
using System.ComponentModel.DataAnnotations;

namespace FileHubOrg.Web.Models.DepartmentViewModels
{
    public class CreateDepartmentViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

