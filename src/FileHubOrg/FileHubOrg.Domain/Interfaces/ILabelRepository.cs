using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Interfaces
{
    public interface ILabelRepository : IGenericRepository<Domain.Entities.File.Label>
    {
        Task<List<Domain.Entities.File.Label>> GetLabelsByUserIdAsync(string userId);
    }
}
