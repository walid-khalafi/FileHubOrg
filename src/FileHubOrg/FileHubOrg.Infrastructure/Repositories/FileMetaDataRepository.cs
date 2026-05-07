using FileHubOrg.Domain.Entities.File;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class FileMetaDataRepository : GenericRepository<FileMetaData>, IFileMetaDataRepository
    {
        private readonly FileHubOrgDbContext _context;
        public FileMetaDataRepository(FileHubOrgDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
