using FileHubOrg.Domain.Entities.Token;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class JWTRepository : GenericRepository<JWT>, IJWTRepository
    {
        private readonly FileHubOrgDbContext _context;
        public JWTRepository(FileHubOrgDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
