using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly string _connectionString;

        public AppUserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public List<AppUser> GetByShop(int shopId)
        {
            List<AppUser> users = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ShopId, DisplayName, PinHash, Role, IsActive FROM AppUsers WHERE ShopId = @ShopId ORDER BY DisplayName",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(MapAppUser(reader));
            }
            return users;
        }

        public AppUser? GetById(int id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ShopId, DisplayName, PinHash, Role, IsActive FROM AppUsers WHERE Id = @Id",
                connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapAppUser(reader) : null;
        }

        public int Create(AppUser appUser)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO AppUsers (ShopId, DisplayName, PinHash, Role, IsActive)
                      VALUES (@ShopId, @DisplayName, @PinHash, @Role, @IsActive);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@ShopId", appUser.ShopId);
                command.Parameters.AddWithValue("@DisplayName", appUser.DisplayName);
                command.Parameters.AddWithValue("@PinHash", appUser.PinHash);
                command.Parameters.AddWithValue("@Role", appUser.Role);
                command.Parameters.AddWithValue("@IsActive", appUser.IsActive);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public bool Update(AppUser appUser)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"UPDATE AppUsers
                      SET DisplayName = @DisplayName, PinHash = @PinHash, Role = @Role, IsActive = @IsActive
                      WHERE Id = @Id",
                    connection);
                command.Parameters.AddWithValue("@Id", appUser.Id);
                command.Parameters.AddWithValue("@DisplayName", appUser.DisplayName);
                command.Parameters.AddWithValue("@PinHash", appUser.PinHash);
                command.Parameters.AddWithValue("@Role", appUser.Role);
                command.Parameters.AddWithValue("@IsActive", appUser.IsActive);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        private static AppUser MapAppUser(SqlDataReader reader)
        {
            return new AppUser
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ShopId = reader.GetInt32(reader.GetOrdinal("ShopId")),
                DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                PinHash = reader.GetString(reader.GetOrdinal("PinHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}
