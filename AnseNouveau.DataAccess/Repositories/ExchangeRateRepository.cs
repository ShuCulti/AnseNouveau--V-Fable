using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class ExchangeRateRepository : IExchangeRateRepository
    {
        private readonly string _connectionString;

        public ExchangeRateRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public List<ExchangeRate> GetByShop(int shopId)
        {
            List<ExchangeRate> rates = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ShopId, CurrencyCode, RateToBase, UpdatedAt FROM ExchangeRates WHERE ShopId = @ShopId ORDER BY CurrencyCode",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rates.Add(MapExchangeRate(reader));
            }
            return rates;
        }

        public ExchangeRate? GetByCurrency(int shopId, string currencyCode)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ShopId, CurrencyCode, RateToBase, UpdatedAt FROM ExchangeRates WHERE ShopId = @ShopId AND CurrencyCode = @CurrencyCode",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@CurrencyCode", currencyCode);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapExchangeRate(reader) : null;
        }

        public int Create(ExchangeRate exchangeRate)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO ExchangeRates (ShopId, CurrencyCode, RateToBase, UpdatedAt)
                      VALUES (@ShopId, @CurrencyCode, @RateToBase, @UpdatedAt);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@ShopId", exchangeRate.ShopId);
                command.Parameters.AddWithValue("@CurrencyCode", exchangeRate.CurrencyCode);
                command.Parameters.AddWithValue("@RateToBase", exchangeRate.RateToBase);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public bool Update(ExchangeRate exchangeRate)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    "UPDATE ExchangeRates SET RateToBase = @RateToBase, UpdatedAt = @UpdatedAt WHERE Id = @Id",
                    connection);
                command.Parameters.AddWithValue("@Id", exchangeRate.Id);
                command.Parameters.AddWithValue("@RateToBase", exchangeRate.RateToBase);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        private static ExchangeRate MapExchangeRate(SqlDataReader reader)
        {
            return new ExchangeRate
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ShopId = reader.GetInt32(reader.GetOrdinal("ShopId")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                RateToBase = reader.GetDecimal(reader.GetOrdinal("RateToBase")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }
    }
}
