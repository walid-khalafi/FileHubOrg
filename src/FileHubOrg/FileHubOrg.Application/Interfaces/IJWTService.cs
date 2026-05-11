using FileHubOrg.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Interfaces
{
    public interface IJWTService
    {
        /// <summary>
        /// Generate download token
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string> GenerateDownloadJwtAsync(Guid fileId, string userId);
        /// <summary>
        /// validate download token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        ClaimsPrincipal? ValidateDownloadToken(string token);
        Task<JWT> GetDownloadTokenAsync(string jwt);
        Task<bool> UpdateDownloadToken(JWT token);
    }
}
