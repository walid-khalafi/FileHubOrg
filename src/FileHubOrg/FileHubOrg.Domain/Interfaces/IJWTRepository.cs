using FileHubOrg.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Interfaces
{
    public interface IJWTRepository :IGenericRepository<JWT>
    {
    }
}
