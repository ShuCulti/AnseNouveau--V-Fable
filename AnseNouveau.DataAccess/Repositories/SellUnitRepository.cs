using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class SellUnitRepository : ISellUnitRepository
    {
        private readonly string _connectionString;

        public SellUnitRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public SellUnit? GetById(int id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ProductId, Label, Price, UnitsPerSale, IsCold, IsActive, SortOrder FROM SellUnits WHERE Id = @Id",
                connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapSellUnit(reader) : null;
        }

        public List<SellUnit> GetByProduct(int productId)
        {
            List<SellUnit> sellUnits = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT Id, ProductId, Label, Price, UnitsPerSale, IsCold, IsActive, SortOrder FROM SellUnits WHERE ProductId = @ProductId ORDER BY SortOrder",
                connection);
            command.Parameters.AddWithValue("@ProductId", productId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                sellUnits.Add(MapSellUnit(reader));
            }
            return sellUnits;
        }

        public int Create(SellUnit sellUnit)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO SellUnits (ProductId, Label, Price, UnitsPerSale, IsCold, IsActive, SortOrder)
                      VALUES (@ProductId, @Label, @Price, @UnitsPerSale, @IsCold, @IsActive, @SortOrder);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@ProductId", sellUnit.ProductId);
                command.Parameters.AddWithValue("@Label", sellUnit.Label);
                command.Parameters.AddWithValue("@Price", sellUnit.Price);
                command.Parameters.AddWithValue("@UnitsPerSale", sellUnit.UnitsPerSale);
                command.Parameters.AddWithValue("@IsCold", sellUnit.IsCold);
                command.Parameters.AddWithValue("@IsActive", sellUnit.IsActive);
                command.Parameters.AddWithValue("@SortOrder", sellUnit.SortOrder);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        // Deliberately does not touch Price; price changes go through UpdatePrice.
        public bool Update(SellUnit sellUnit)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"UPDATE SellUnits
                      SET Label = @Label, UnitsPerSale = @UnitsPerSale, IsCold = @IsCold,
                          IsActive = @IsActive, SortOrder = @SortOrder
                      WHERE Id = @Id",
                    connection);
                command.Parameters.AddWithValue("@Id", sellUnit.Id);
                command.Parameters.AddWithValue("@Label", sellUnit.Label);
                command.Parameters.AddWithValue("@UnitsPerSale", sellUnit.UnitsPerSale);
                command.Parameters.AddWithValue("@IsCold", sellUnit.IsCold);
                command.Parameters.AddWithValue("@IsActive", sellUnit.IsActive);
                command.Parameters.AddWithValue("@SortOrder", sellUnit.SortOrder);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        // Price update and PriceHistory row are one transaction so history can never be skipped.
        public bool UpdatePrice(int sellUnitId, decimal newPrice, int changedByUserId)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                using SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    decimal? oldPrice;
                    using (SqlCommand selectCommand = new(
                        "SELECT Price FROM SellUnits WHERE Id = @Id", connection, transaction))
                    {
                        selectCommand.Parameters.AddWithValue("@Id", sellUnitId);
                        object? result = selectCommand.ExecuteScalar();
                        oldPrice = result == null ? null : (decimal)result;
                    }
                    if (!oldPrice.HasValue)
                    {
                        transaction.Rollback();
                        return false;
                    }
                    if (oldPrice.Value == newPrice)
                    {
                        transaction.Rollback();
                        return true;
                    }

                    using (SqlCommand updateCommand = new(
                        "UPDATE SellUnits SET Price = @Price WHERE Id = @Id", connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@Id", sellUnitId);
                        updateCommand.Parameters.AddWithValue("@Price", newPrice);
                        updateCommand.ExecuteNonQuery();
                    }

                    using (SqlCommand historyCommand = new(
                        @"INSERT INTO PriceHistory (SellUnitId, OldPrice, NewPrice, ChangedByUserId, ChangedAt)
                          VALUES (@SellUnitId, @OldPrice, @NewPrice, @ChangedByUserId, @ChangedAt)",
                        connection, transaction))
                    {
                        historyCommand.Parameters.AddWithValue("@SellUnitId", sellUnitId);
                        historyCommand.Parameters.AddWithValue("@OldPrice", oldPrice.Value);
                        historyCommand.Parameters.AddWithValue("@NewPrice", newPrice);
                        historyCommand.Parameters.AddWithValue("@ChangedByUserId", changedByUserId);
                        historyCommand.Parameters.AddWithValue("@ChangedAt", DateTime.UtcNow);
                        historyCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
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

        private static SellUnit MapSellUnit(SqlDataReader reader)
        {
            return new SellUnit
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                Label = reader.GetString(reader.GetOrdinal("Label")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                UnitsPerSale = reader.GetDecimal(reader.GetOrdinal("UnitsPerSale")),
                IsCold = reader.GetBoolean(reader.GetOrdinal("IsCold")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
            };
        }
    }
}
