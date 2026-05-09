using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.File;
using FileHubOrg.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Services
{
    public class LabelService : ILabelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _ipAddress;
        public LabelService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        public async Task<bool> CreateLabelAsync(string userId, string labelName)
        {
            _unitOfWork.Labels.AddAsync(new Label()
            {
                Name = labelName,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                CreatedByIP = _ipAddress,
            });

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async Task<Label> GetLabelByIdAsync(Guid id) => await _unitOfWork.Labels.GetFirstOrDefaultAsync(x => x.Id.Equals(id));

        public async Task<List<Label>> GetLabelsAsync(string userId) => await _unitOfWork.Labels.GetLabelsByUserIdAsync(userId);

        public async Task<bool> RemoveLabelAsync(string userId, string labelId)
        {
            var label = await _unitOfWork.Labels.GetFirstOrDefaultAsync(x => x.Id.Equals(labelId));
            if (label != null)
            {
                var files = await _unitOfWork.FileMetaData.GetFirstOrDefaultAsync(x => x.LabelId.Equals(labelId));
                if (files != null)
                {
                    return false;
                }
               
                await _unitOfWork.Labels.RemoveAsync(label);
                await _unitOfWork.SaveChangesAsync();
            }
            return false;
        }

        public async Task<bool> UpdateLabelAsync(string userId, string labelId, string newName)
        {
            var label = await _unitOfWork.Labels.GetFirstOrDefaultAsync(x => x.Id.Equals(labelId));
            if (label != null)
            {
                label.Name = newName;
                label.UpdatedAt = DateTime.Now;
                label.UpdatedBy = userId;
                label.UpdatedByIP = _ipAddress;
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
