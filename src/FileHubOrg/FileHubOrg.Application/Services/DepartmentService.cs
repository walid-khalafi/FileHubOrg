using FileHubOrg.Application.Interfaces;
using FileHubOrg.Domain.Entities.Organization;
using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Application.Services
{
    public class DepartmentService : IDepartmentService
    {

        private readonly IUnitOfWork _unitOfWork;
        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IReadOnlyList<Department>> GetDepartmentsAsync() => await _unitOfWork.Departments.GetAllAsync();

        public async Task<Department> GetDepartmentsByIdAsync(Guid id) => await _unitOfWork.Departments.GetFirstOrDefaultAsync(x => x.Id.Equals(id));

        public async Task<List<ApplicationUser>> GetDepartmentUsersAsync(Guid id) => await _unitOfWork.ApplicationUsers.GetByDepartmentAsync(id);
    }
}
