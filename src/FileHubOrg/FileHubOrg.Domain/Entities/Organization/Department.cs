using FileHubOrg.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Entities.Organization
{
    public class Department :BaseEntity
    {
        /// <summary>
        /// Get or sets the department's name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Department members list
        /// </summary>
        public virtual ICollection<ApplicationUser> Members { get; set; } = [];
      
    }
}
