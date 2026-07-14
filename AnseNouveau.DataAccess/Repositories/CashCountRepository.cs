using AnseNouveau.DataAccess.Extensions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class CashCountRepository : ICashCountRepository
    {
        private readonly string _connectionString;

        public CashCountRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public int Create(CashCount cashCount)
        {
            try
            {
                // Difference is a persisted computed column; it is never inserted.
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO CashCounts (ShopId, UserId, BusinessDate, FloatAmount, ExpectedCash, CountedCash, ClosedAt, Notes)
                      VALUES (@ShopId, @UserId, @BusinessDate, @FloatAmount, @ExpectedCash, @CountedCash, @ClosedAt, @Notes);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@ShopId", cashCount.ShopId);
                command.Parameters.AddWithValue("@UserId", cashCount.UserId);
                command.Parameters.AddWithValue("@BusinessDate", cashCount.BusinessDate.Date);
                command.Parameters.AddWithValue("@FloatAmount", cashCount.FloatAmount);
                command.Parameters.AddWithValue("@ExpectedCash", cashCount.ExpectedCash);
                command.Parameters.AddWithValue("@CountedCash", cashCount.CountedCash);
                command.Parameters.AddWithValue("@ClosedAt", cashCount.ClosedAt);
                command.Parameters.AddWithValue("@Notes", (object?)cashCount.Notes ?? DBNull.Value);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public CashCount? GetByBusinessDate(int shopId, DateTime businessDate)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT Id, ShopId, UserId, BusinessDate, FloatAmount, ExpectedCash, CountedCash, Difference, ClosedAt, Notes
                  FROM CashCounts WHERE ShopId = @ShopId AND BusinessDate = @BusinessDate",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@BusinessDate", businessDate.Date);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapCashCount(reader) : null;
        }

        private static CashCount MapCashCount(SqlDataReader reader)
        {
            return new CashCount
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ShopId = reader.GetInt32(reader.GetOrdinal("ShopId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                BusinessDate = reader.GetDateTime(reader.GetOrdinal("BusinessDate")),
                FloatAmount = reader.GetDecimal(reader.GetOrdinal("FloatAmount")),
                ExpectedCash = reader.GetDecimal(reader.GetOrdinal("ExpectedCash")),
                CountedCash = reader.GetDecimal(reader.GetOrdinal("CountedCash")),
                Difference = reader.GetDecimal(reader.GetOrdinal("Difference")),
                ClosedAt = reader.GetDateTime(reader.GetOrdinal("ClosedAt")),
                Notes = reader.GetStringOrNull("Notes")
            };
        }
    }
}
