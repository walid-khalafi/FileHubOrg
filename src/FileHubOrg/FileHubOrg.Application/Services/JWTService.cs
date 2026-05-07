using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.Token;
using FileHubOrg.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Services
{
    public class JWTService : IJWTService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _secret;
        private readonly TimeSpan _tokenLifetime;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public JWTService(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _secret = configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret");
            _tokenLifetime = TimeSpan.FromMinutes(5);
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> GenerateDownloadJwtAsync(Guid fileId, string userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var claims = new List<Claim>
        {
            new Claim("fileId", fileId.ToString()),
            new Claim("userId", userId)
        };

            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.Add(_tokenLifetime),
                signingCredentials: credentials);

            string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var jwt = new JWT()
            {
                Id = Guid.NewGuid(),
                CreatedAt = now,
                CreatedBy = userId,
                FileMetadataId = fileId,
                CreatedByIP = ipAddress,
                IsUsed = false,
                Jwt = new JwtSecurityTokenHandler().WriteToken(token),
            };

            try
            {
                await _unitOfWork.JWTs.AddAsync(jwt);
                await _unitOfWork.SaveChangesAsync();
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }
            return string.Empty;
        }

        public ClaimsPrincipal? ValidateDownloadToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                return handler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
