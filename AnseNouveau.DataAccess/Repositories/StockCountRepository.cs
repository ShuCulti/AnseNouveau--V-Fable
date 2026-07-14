using AnseNouveau.DataAccess.Extensions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class StockCountRepository : IStockCountRepository
    {
        private readonly string _connectionString;

        public StockCountRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public int Create(StockCount stockCount)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO StockCounts (ShopId, UserId, StartedAt, Notes)
                      VALUES (@ShopId, @UserId, @StartedAt, @Notes);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@ShopId", stockCount.ShopId);
                command.Parameters.AddWithValue("@UserId", stockCount.UserId);
                command.Parameters.AddWithValue("@StartedAt", stockCount.StartedAt);
                command.Parameters.AddWithValue("@Notes", (object?)stockCount.Notes ?? DBNull.Value);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public StockCount? GetById(int id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ShopId, UserId, StartedAt, ClosedAt, Notes FROM StockCounts WHERE Id = @Id",
                connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapStockCount(reader) : null;
        }

        public StockCount? GetOpenByShop(int shopId)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT TOP 1 Id, ShopId, UserId, StartedAt, ClosedAt, Notes
                  FROM StockCounts WHERE ShopId = @ShopId AND ClosedAt IS NULL
                  ORDER BY StartedAt DESC",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapStockCount(reader) : null;
        }

        public bool Close(int id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "UPDATE StockCounts SET ClosedAt = @ClosedAt WHERE Id = @Id AND ClosedAt IS NULL",
                connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@ClosedAt", DateTime.UtcNow);
            connection.Open();
            return command.ExecuteNonQuery() > 0;
        }

        // Rescanning a product updates CountedQty but keeps the ExpectedQty frozen at first scan.
        public int UpsertLine(StockCountLine line)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"UPDATE StockCountLines SET CountedQty = @CountedQty
                      WHERE StockCountId = @StockCountId AND ProductId = @ProductId;
                      IF @@ROWCOUNT = 0
                          INSERT INTO StockCountLines (StockCountId, ProductId, ExpectedQty, CountedQty)
                          VALUES (@StockCountId, @ProductId, @ExpectedQty, @CountedQty);
                      SELECT Id FROM StockCountLines WHERE StockCountId = @StockCountId AND ProductId = @ProductId;",
                    connection);
                command.Parameters.AddWithValue("@StockCountId", line.StockCountId);
                command.Parameters.AddWithValue("@ProductId", line.ProductId);
                command.Parameters.AddWithValue("@ExpectedQty", line.ExpectedQty);
                command.Parameters.AddWithValue("@CountedQty", line.CountedQty);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public List<StockCountLine> GetLines(int stockCountId)
        {
            List<StockCountLine> lines = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT Id, StockCountId, ProductId, ExpectedQty, CountedQty
                  FROM StockCountLines WHERE StockCountId = @StockCountId ORDER BY Id",
                connection);
            command.Parameters.AddWithValue("@StockCountId", stockCountId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(MapStockCountLine(reader));
            }
            return lines;
        }

        public List<StockCountDiffRow> GetDifferenceReport(int stockCountId)
        {
            List<StockCountDiffRow> rows = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT l.ProductId, p.Name AS ProductName, l.ExpectedQty, l.CountedQty,
                         l.CountedQty - l.ExpectedQty AS Difference
                  FROM StockCountLines l
                  JOIN Products p ON p.Id = l.ProductId
                  WHERE l.StockCountId = @StockCountId
                  ORDER BY p.Name",
                connection);
            command.Parameters.AddWithValue("@StockCountId", stockCountId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new StockCountDiffRow
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    ExpectedQty = reader.GetDecimal(reader.GetOrdinal("ExpectedQty")),
                    CountedQty = reader.GetDecimal(reader.GetOrdinal("CountedQty")),
                    Difference = reader.GetDecimal(reader.GetOrdinal("Difference"))
                });
            }
            return rows;
        }

        private static StockCount MapStockCount(SqlDataReader reader)
        {
            return new StockCount
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ShopId = reader.GetInt32(reader.GetOrdinal("ShopId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                StartedAt = reader.GetDateTime(reader.GetOrdinal("StartedAt")),
                ClosedAt = reader.GetDateTimeOrNull("ClosedAt"),
                Notes = reader.GetStringOrNull("Notes")
            };
        }

        private static StockCountLine MapStockCountLine(SqlDataReader reader)
        {
            return new StockCountLine
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StockCountId = reader.GetInt32(reader.GetOrdinal("StockCountId")),
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                ExpectedQty = reader.GetDecimal(reader.GetOrdinal("ExpectedQty")),
                CountedQty = reader.GetDecimal(reader.GetOrdinal("CountedQty"))
            };
        }
    }
}
