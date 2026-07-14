using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public List<Department> GetByShop(int shopId)
        {
            return _departmentRepository.GetByShop(shopId);
        }

        public Department? GetById(int id)
        {
            return _departmentRepository.GetById(id);
        }

        public int Create(Department department)
        {
            return _departmentRepository.Create(department);
        }

        public bool Update(Department department)
        {
            return _departmentRepository.Update(department);
        }
    }
}
