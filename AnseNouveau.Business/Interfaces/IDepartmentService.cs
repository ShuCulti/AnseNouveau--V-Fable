using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface IDepartmentService
    {
        List<Department> GetByShop(int shopId);
        Department? GetById(int id);
        int Create(Department department);
        bool Update(Department department);
    }
}
