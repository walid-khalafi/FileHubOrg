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

        public async Task<Department> CreateDepartmentAsync(string name, string createdBy, string createdByIp)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Department name is required.", nameof(name));

            var department = new Department
            {
                Name = name.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? string.Empty : createdBy,
                CreatedByIP = string.IsNullOrWhiteSpace(createdByIp) ? string.Empty : createdByIp
            };

            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();
            return department;
        }

        public async Task<bool> UpdateDepartmentAsync(Guid id, string name, string updatedBy, string updatedByIp)
        {
            if (id == Guid.Empty) return false;
            if (string.IsNullOrWhiteSpace(name)) return false;

            var department = await _unitOfWork.Departments.GetFirstOrDefaultAsync(x => x.Id == id);
            if (department == null) return false;

            department.Name = name.Trim();
            department.UpdatedBy = updatedBy;
            department.UpdatedByIP = updatedByIp;
            department.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Departments.UpdateAsync(department);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepartmentAsync(Guid id)
        {
            if (id == Guid.Empty) return false;

            var department = await _unitOfWork.Departments.GetFirstOrDefaultAsync(x => x.Id == id);
            if (department == null) return false;

            await _unitOfWork.Departments.RemoveAsync(department);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
