using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class PriceHistoryRepository : IPriceHistoryRepository
    {
        private readonly string _connectionString;

        public PriceHistoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public List<PriceHistory> GetBySellUnit(int sellUnitId)
        {
            List<PriceHistory> history = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, SellUnitId, OldPrice, NewPrice, ChangedByUserId, ChangedAt FROM PriceHistory WHERE SellUnitId = @SellUnitId ORDER BY ChangedAt DESC",
                connection);
            command.Parameters.AddWithValue("@SellUnitId", sellUnitId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                history.Add(MapPriceHistory(reader));
            }
            return history;
        }

        private static PriceHistory MapPriceHistory(SqlDataReader reader)
        {
            return new PriceHistory
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SellUnitId = reader.GetInt32(reader.GetOrdinal("SellUnitId")),
                OldPrice = reader.GetDecimal(reader.GetOrdinal("OldPrice")),
                NewPrice = reader.GetDecimal(reader.GetOrdinal("NewPrice")),
                ChangedByUserId = reader.GetInt32(reader.GetOrdinal("ChangedByUserId")),
                ChangedAt = reader.GetDateTime(reader.GetOrdinal("ChangedAt"))
            };
        }
    }
}
