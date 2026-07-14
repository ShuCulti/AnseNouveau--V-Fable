using AnseNouveau.DataAccess.Extensions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly string _connectionString;

        public ShopRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public List<Shop> GetAll()
        {
            List<Shop> shops = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, Name, ReceiptHeader, ReceiptFooter, BaseCurrency, TaxRatePercent, CreatedAt FROM Shops",
                connection);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                shops.Add(MapShop(reader));
            }
            return shops;
        }

        public Shop? GetById(int id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, Name, ReceiptHeader, ReceiptFooter, BaseCurrency, TaxRatePercent, CreatedAt FROM Shops WHERE Id = @Id",
                connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapShop(reader) : null;
        }

        public int Create(Shop shop)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO Shops (Name, ReceiptHeader, ReceiptFooter, BaseCurrency, TaxRatePercent)
                      VALUES (@Name, @ReceiptHeader, @ReceiptFooter, @BaseCurrency, @TaxRatePercent);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@Name", shop.Name);
                command.Parameters.AddWithValue("@ReceiptHeader", (object?)shop.ReceiptHeader ?? DBNull.Value);
                command.Parameters.AddWithValue("@ReceiptFooter", (object?)shop.ReceiptFooter ?? DBNull.Value);
                command.Parameters.AddWithValue("@BaseCurrency", shop.BaseCurrency);
                command.Parameters.AddWithValue("@TaxRatePercent", shop.TaxRatePercent);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public bool Update(Shop shop)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"UPDATE Shops
                      SET Name = @Name, ReceiptHeader = @ReceiptHeader, ReceiptFooter = @ReceiptFooter,
                          BaseCurrency = @BaseCurrency, TaxRatePercent = @TaxRatePercent
                      WHERE Id = @Id",
                    connection);
                command.Parameters.AddWithValue("@Id", shop.Id);
                command.Parameters.AddWithValue("@Name", shop.Name);
                command.Parameters.AddWithValue("@ReceiptHeader", (object?)shop.ReceiptHeader ?? DBNull.Value);
                command.Parameters.AddWithValue("@ReceiptFooter", (object?)shop.ReceiptFooter ?? DBNull.Value);
                command.Parameters.AddWithValue("@BaseCurrency", shop.BaseCurrency);
                command.Parameters.AddWithValue("@TaxRatePercent", shop.TaxRatePercent);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        private static Shop MapShop(SqlDataReader reader)
        {
            return new Shop
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                ReceiptHeader = reader.GetStringOrNull("ReceiptHeader"),
                ReceiptFooter = reader.GetStringOrNull("ReceiptFooter"),
                BaseCurrency = reader.GetString(reader.GetOrdinal("BaseCurrency")),
                TaxRatePercent = reader.GetDecimal(reader.GetOrdinal("TaxRatePercent")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
