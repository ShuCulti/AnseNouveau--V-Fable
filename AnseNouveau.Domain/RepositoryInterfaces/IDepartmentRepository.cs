using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface IDepartmentRepository
    {
        List<Department> GetByShop(int shopId);
        Department? GetById(int id);
        int Create(Department department);
        bool Update(Department department);
    }
}
