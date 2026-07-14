using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string _connectionString;

        public DepartmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public List<Department> GetByShop(int shopId)
        {
            List<Department> departments = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ShopId, Name, SortOrder FROM Departments WHERE ShopId = @ShopId ORDER BY SortOrder, Name",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                departments.Add(MapDepartment(reader));
            }
            return departments;
        }

        public Department? GetById(int id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ShopId, Name, SortOrder FROM Departments WHERE Id = @Id",
                connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapDepartment(reader) : null;
        }

        public int Create(Department department)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO Departments (ShopId, Name, SortOrder)
                      VALUES (@ShopId, @Name, @SortOrder);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@ShopId", department.ShopId);
                command.Parameters.AddWithValue("@Name", department.Name);
                command.Parameters.AddWithValue("@SortOrder", department.SortOrder);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public bool Update(Department department)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    "UPDATE Departments SET Name = @Name, SortOrder = @SortOrder WHERE Id = @Id",
                    connection);
                command.Parameters.AddWithValue("@Id", department.Id);
                command.Parameters.AddWithValue("@Name", department.Name);
                command.Parameters.AddWithValue("@SortOrder", department.SortOrder);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        private static Department MapDepartment(SqlDataReader reader)
        {
            return new Department
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ShopId = reader.GetInt32(reader.GetOrdinal("ShopId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
            };
        }
    }
}
