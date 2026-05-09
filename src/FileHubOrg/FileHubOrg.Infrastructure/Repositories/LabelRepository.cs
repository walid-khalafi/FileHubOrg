using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class LabelRepository : GenericRepository<Domain.Entities.File.Label>, ILabelRepository
    {
        private readonly FileHubOrgDbContext _context;
        public LabelRepository(FileHubOrgDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Get user labels
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<Domain.Entities.File.Label>> GetLabelsByUserIdAsync(string userId)
            => await _context.Labels.Where(x => x.CreatedBy.Equals(userId)).ToListAsync();
    }
}
