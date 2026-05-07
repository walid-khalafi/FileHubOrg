using FileHubOrg.Domain.Entities.File;
using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class FileMemberRepository : GenericRepository<FileMember>, IFileMemberRepository
    {
        private readonly FileHubOrgDbContext _context;
        public FileMemberRepository(FileHubOrgDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
