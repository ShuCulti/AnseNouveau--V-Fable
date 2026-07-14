using AnseNouveau.DataAccess.Extensions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    // StockMovements is an append-only ledger: this repository only ever inserts.
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly string _connectionString;

        public StockMovementRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        // Movements and their StockQty updates commit together or not at all.
        public void AddRange(IEnumerable<StockMovement> movements)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                using SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    foreach (StockMovement movement in movements)
                    {
                        StockMovementSql.Insert(connection, transaction, movement);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public List<StockMovement> GetByProduct(int productId, DateTime fromUtc, DateTime toUtc)
        {
            List<StockMovement> movements = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT Id, ProductId, QtyDelta, MovementType, SaleId, StockCountId, Reason, UserId, MovedAt
                  FROM StockMovements
                  WHERE ProductId = @ProductId AND MovedAt >= @FromUtc AND MovedAt < @ToUtc
                  ORDER BY MovedAt DESC",
                connection);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.AddWithValue("@FromUtc", fromUtc);
            command.Parameters.AddWithValue("@ToUtc", toUtc);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                movements.Add(MapStockMovement(reader));
            }
            return movements;
        }

        public List<StockReportRow> GetStockReport(int shopId, DateTime fromUtc, DateTime toUtc)
        {
            List<StockReportRow> rows = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT p.Id AS ProductId, p.Name AS ProductName,
                         ISNULL(SUM(CASE WHEN m.MovedAt < @FromUtc THEN m.QtyDelta END), 0) AS OpeningQty,
                         ISNULL(SUM(CASE WHEN m.MovedAt >= @FromUtc AND m.MovementType IN ('Sale', 'Refund') THEN -m.QtyDelta END), 0) AS SoldQty,
                         ISNULL(SUM(CASE WHEN m.MovedAt >= @FromUtc AND m.MovementType = 'Delivery' THEN m.QtyDelta END), 0) AS DeliveredQty,
                         ISNULL(SUM(CASE WHEN m.MovedAt >= @FromUtc AND m.MovementType IN ('Adjustment', 'CountFix') THEN m.QtyDelta END), 0) AS AdjustedQty,
                         ISNULL(SUM(m.QtyDelta), 0) AS ClosingQty
                  FROM Products p
                  LEFT JOIN StockMovements m ON m.ProductId = p.Id AND m.MovedAt < @ToUtc
                  WHERE p.ShopId = @ShopId AND p.IsActive = 1
                  GROUP BY p.Id, p.Name
                  ORDER BY p.Name",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@FromUtc", fromUtc);
            command.Parameters.AddWithValue("@ToUtc", toUtc);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new StockReportRow
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    OpeningQty = reader.GetDecimal(reader.GetOrdinal("OpeningQty")),
                    SoldQty = reader.GetDecimal(reader.GetOrdinal("SoldQty")),
                    DeliveredQty = reader.GetDecimal(reader.GetOrdinal("DeliveredQty")),
                    AdjustedQty = reader.GetDecimal(reader.GetOrdinal("AdjustedQty")),
                    ClosingQty = reader.GetDecimal(reader.GetOrdinal("ClosingQty"))
                });
            }
            return rows;
        }

        public List<StockAtDateRow> GetStockAtDate(int shopId, DateTime atUtc)
        {
            List<StockAtDateRow> rows = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT p.Id AS ProductId, p.Name AS ProductName, ISNULL(SUM(m.QtyDelta), 0) AS Qty
                  FROM Products p
                  LEFT JOIN StockMovements m ON m.ProductId = p.Id AND m.MovedAt < @AtUtc
                  WHERE p.ShopId = @ShopId AND p.IsActive = 1
                  GROUP BY p.Id, p.Name
                  ORDER BY p.Name",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@AtUtc", atUtc);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new StockAtDateRow
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    Qty = reader.GetDecimal(reader.GetOrdinal("Qty"))
                });
            }
            return rows;
        }

        private static StockMovement MapStockMovement(SqlDataReader reader)
        {
            return new StockMovement
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                QtyDelta = reader.GetDecimal(reader.GetOrdinal("QtyDelta")),
                MovementType = reader.GetString(reader.GetOrdinal("MovementType")),
                SaleId = reader.GetInt32OrNull("SaleId"),
                StockCountId = reader.GetInt32OrNull("StockCountId"),
                Reason = reader.GetStringOrNull("Reason"),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                MovedAt = reader.GetDateTime(reader.GetOrdinal("MovedAt"))
            };
        }
    }
}
