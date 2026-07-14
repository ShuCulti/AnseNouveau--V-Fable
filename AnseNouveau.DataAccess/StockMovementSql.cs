using AnseNouveau.Domain.Models;
using Microsoft.Data.SqlClient;

namespace AnseNouveau.DataAccess
{
    // Shared by SaleRepository and StockMovementRepository so a movement row and the
    // matching Products.StockQty update always happen together in the caller's transaction.
    internal static class StockMovementSql
    {
        public static void Insert(SqlConnection connection, SqlTransaction transaction, StockMovement movement)
        {
            using SqlCommand command = new(
                @"INSERT INTO StockMovements (ProductId, QtyDelta, MovementType, SaleId, StockCountId, Reason, UserId, MovedAt)
                  VALUES (@ProductId, @QtyDelta, @MovementType, @SaleId, @StockCountId, @Reason, @UserId, @MovedAt);
                  UPDATE Products SET StockQty = StockQty + @QtyDelta WHERE Id = @ProductId;",
                connection, transaction);
            command.Parameters.AddWithValue("@ProductId", movement.ProductId);
            command.Parameters.AddWithValue("@QtyDelta", movement.QtyDelta);
            command.Parameters.AddWithValue("@MovementType", movement.MovementType);
            command.Parameters.AddWithValue("@SaleId", (object?)movement.SaleId ?? DBNull.Value);
            command.Parameters.AddWithValue("@StockCountId", (object?)movement.StockCountId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Reason", (object?)movement.Reason ?? DBNull.Value);
            command.Parameters.AddWithValue("@UserId", movement.UserId);
            command.Parameters.AddWithValue("@MovedAt", movement.MovedAt);
            command.ExecuteNonQuery();
        }
    }
}
