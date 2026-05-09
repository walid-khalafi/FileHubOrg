using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Interfaces
{
    public interface ILabelService
    {

        Task<List<Domain.Entities.File.Label>> GetLabelsAsync(string userId);
        Task<Domain.Entities.File.Label> GetLabelByIdAsync(Guid id);
        Task<bool> CreateLabelAsync(string userId, string labelName);
        Task<bool> RemoveLabelAsync(string userId, string labelId);
        Task<bool> UpdateLabelAsync(string userId, string labelId,string newName);

    }
}
