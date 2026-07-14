using AnseNouveau.DataAccess.Extensions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    // Sales and SaleLines are immutable: this repository only ever inserts them.
    public class SaleRepository : ISaleRepository
    {
        private readonly string _connectionString;

        public SaleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        // One transaction: sale, lines, stock movements and the StockQty updates all commit or none do.
        public int Create(Sale sale, IEnumerable<StockMovement> movements)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                using SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    int saleId;
                    using (SqlCommand saleCommand = new(
                        @"INSERT INTO Sales (ShopId, UserId, SaleTimeUtc, Status, RefundOfSaleId, TotalAmount,
                                             PaymentMethod, TenderCurrency, TenderRate, AmountTendered, ChangeGiven)
                          VALUES (@ShopId, @UserId, @SaleTimeUtc, @Status, @RefundOfSaleId, @TotalAmount,
                                  @PaymentMethod, @TenderCurrency, @TenderRate, @AmountTendered, @ChangeGiven);
                          SELECT CAST(SCOPE_IDENTITY() AS int);",
                        connection, transaction))
                    {
                        saleCommand.Parameters.AddWithValue("@ShopId", sale.ShopId);
                        saleCommand.Parameters.AddWithValue("@UserId", sale.UserId);
                        saleCommand.Parameters.AddWithValue("@SaleTimeUtc", sale.SaleTimeUtc);
                        saleCommand.Parameters.AddWithValue("@Status", sale.Status);
                        saleCommand.Parameters.AddWithValue("@RefundOfSaleId", (object?)sale.RefundOfSaleId ?? DBNull.Value);
                        saleCommand.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                        saleCommand.Parameters.AddWithValue("@PaymentMethod", sale.PaymentMethod);
                        saleCommand.Parameters.AddWithValue("@TenderCurrency", sale.TenderCurrency);
                        saleCommand.Parameters.AddWithValue("@TenderRate", sale.TenderRate);
                        saleCommand.Parameters.AddWithValue("@AmountTendered", (object?)sale.AmountTendered ?? DBNull.Value);
                        saleCommand.Parameters.AddWithValue("@ChangeGiven", (object?)sale.ChangeGiven ?? DBNull.Value);
                        saleId = Convert.ToInt32(saleCommand.ExecuteScalar());
                    }

                    using (SqlCommand lineCommand = new(
                        @"INSERT INTO SaleLines (SaleId, SellUnitId, NameSnapshot, PriceSnapshot, Qty, LineTotal)
                          VALUES (@SaleId, @SellUnitId, @NameSnapshot, @PriceSnapshot, @Qty, @LineTotal)",
                        connection, transaction))
                    {
                        foreach (SaleLine line in sale.Lines)
                        {
                            lineCommand.Parameters.Clear();
                            lineCommand.Parameters.AddWithValue("@SaleId", saleId);
                            lineCommand.Parameters.AddWithValue("@SellUnitId", (object?)line.SellUnitId ?? DBNull.Value);
                            lineCommand.Parameters.AddWithValue("@NameSnapshot", line.NameSnapshot);
                            lineCommand.Parameters.AddWithValue("@PriceSnapshot", line.PriceSnapshot);
                            lineCommand.Parameters.AddWithValue("@Qty", line.Qty);
                            lineCommand.Parameters.AddWithValue("@LineTotal", line.LineTotal);
                            lineCommand.ExecuteNonQuery();
                        }
                    }

                    foreach (StockMovement movement in movements)
                    {
                        movement.SaleId = saleId;
                        StockMovementSql.Insert(connection, transaction, movement);
                    }

                    transaction.Commit();
                    sale.Id = saleId;
                    return saleId;
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

        public Sale? GetById(int id)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            Sale? sale;
            using (SqlCommand saleCommand = new(
                @"SELECT Id, ShopId, UserId, SaleTimeUtc, Status, RefundOfSaleId, TotalAmount,
                         PaymentMethod, TenderCurrency, TenderRate, AmountTendered, ChangeGiven
                  FROM Sales WHERE Id = @Id",
                connection))
            {
                saleCommand.Parameters.AddWithValue("@Id", id);
                using SqlDataReader reader = saleCommand.ExecuteReader();
                sale = reader.Read() ? MapSale(reader) : null;
            }
            if (sale == null)
            {
                return null;
            }

            using (SqlCommand lineCommand = new(
                @"SELECT Id, SaleId, SellUnitId, NameSnapshot, PriceSnapshot, Qty, LineTotal
                  FROM SaleLines WHERE SaleId = @SaleId ORDER BY Id",
                connection))
            {
                lineCommand.Parameters.AddWithValue("@SaleId", id);
                using SqlDataReader reader = lineCommand.ExecuteReader();
                while (reader.Read())
                {
                    sale.Lines.Add(MapSaleLine(reader));
                }
            }
            return sale;
        }

        public List<Sale> GetByDateRange(int shopId, DateTime fromUtc, DateTime toUtc)
        {
            List<Sale> sales = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT Id, ShopId, UserId, SaleTimeUtc, Status, RefundOfSaleId, TotalAmount,
                         PaymentMethod, TenderCurrency, TenderRate, AmountTendered, ChangeGiven
                  FROM Sales
                  WHERE ShopId = @ShopId AND SaleTimeUtc >= @FromUtc AND SaleTimeUtc < @ToUtc
                  ORDER BY SaleTimeUtc DESC",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@FromUtc", fromUtc);
            command.Parameters.AddWithValue("@ToUtc", toUtc);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                sales.Add(MapSale(reader));
            }
            return sales;
        }

        public bool HasRefund(int saleId)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                "SELECT COUNT(1) FROM Sales WHERE RefundOfSaleId = @SaleId",
                connection);
            command.Parameters.AddWithValue("@SaleId", saleId);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public ZReport GetZReport(int shopId, DateTime fromUtc, DateTime toUtc)
        {
            ZReport report = new();
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using (SqlCommand totalsCommand = new(
                @"SELECT ISNULL(SUM(CASE WHEN Status = 'Completed' THEN TotalAmount END), 0) AS SalesTotal,
                         ISNULL(SUM(CASE WHEN Status = 'Refund' THEN TotalAmount END), 0) AS RefundTotal,
                         ISNULL(SUM(TotalAmount), 0) AS NetTotal,
                         ISNULL(SUM(CASE WHEN PaymentMethod = 'Cash' THEN TotalAmount END), 0) AS CashTotal,
                         ISNULL(SUM(CASE WHEN PaymentMethod = 'Card' THEN TotalAmount END), 0) AS CardTotal,
                         ISNULL(SUM(CASE WHEN PaymentMethod = 'Other' THEN TotalAmount END), 0) AS OtherTotal,
                         SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS SaleCount
                  FROM Sales
                  WHERE ShopId = @ShopId AND SaleTimeUtc >= @FromUtc AND SaleTimeUtc < @ToUtc",
                connection))
            {
                totalsCommand.Parameters.AddWithValue("@ShopId", shopId);
                totalsCommand.Parameters.AddWithValue("@FromUtc", fromUtc);
                totalsCommand.Parameters.AddWithValue("@ToUtc", toUtc);
                using SqlDataReader reader = totalsCommand.ExecuteReader();
                if (reader.Read())
                {
                    report.SalesTotal = reader.GetDecimal(reader.GetOrdinal("SalesTotal"));
                    report.RefundTotal = reader.GetDecimal(reader.GetOrdinal("RefundTotal"));
                    report.NetTotal = reader.GetDecimal(reader.GetOrdinal("NetTotal"));
                    report.CashTotal = reader.GetDecimal(reader.GetOrdinal("CashTotal"));
                    report.CardTotal = reader.GetDecimal(reader.GetOrdinal("CardTotal"));
                    report.OtherTotal = reader.GetDecimal(reader.GetOrdinal("OtherTotal"));
                    report.SaleCount = reader.GetInt32OrNull("SaleCount") ?? 0;
                }
            }

            using (SqlCommand departmentCommand = new(
                @"SELECT d.Id AS DepartmentId, ISNULL(d.Name, 'Other') AS DepartmentName, SUM(sl.LineTotal) AS Total
                  FROM SaleLines sl
                  JOIN Sales s ON s.Id = sl.SaleId
                  LEFT JOIN SellUnits su ON su.Id = sl.SellUnitId
                  LEFT JOIN Products p ON p.Id = su.ProductId
                  LEFT JOIN Departments d ON d.Id = p.DepartmentId
                  WHERE s.ShopId = @ShopId AND s.SaleTimeUtc >= @FromUtc AND s.SaleTimeUtc < @ToUtc
                  GROUP BY d.Id, d.Name
                  ORDER BY Total DESC",
                connection))
            {
                departmentCommand.Parameters.AddWithValue("@ShopId", shopId);
                departmentCommand.Parameters.AddWithValue("@FromUtc", fromUtc);
                departmentCommand.Parameters.AddWithValue("@ToUtc", toUtc);
                using SqlDataReader reader = departmentCommand.ExecuteReader();
                while (reader.Read())
                {
                    report.Departments.Add(new DepartmentSalesRow
                    {
                        DepartmentId = reader.GetInt32OrNull("DepartmentId"),
                        DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                        Total = reader.GetDecimal(reader.GetOrdinal("Total"))
                    });
                }
            }
            return report;
        }

        public List<ProfitReportRow> GetProfitReport(int shopId, DateTime fromUtc, DateTime toUtc)
        {
            List<ProfitReportRow> rows = new();
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT p.Id AS ProductId, p.Name AS ProductName,
                         SUM(sl.Qty * su.UnitsPerSale) AS UnitsSold,
                         SUM(sl.LineTotal) AS Revenue,
                         SUM(sl.Qty * su.UnitsPerSale * p.CostPrice) AS Cost
                  FROM SaleLines sl
                  JOIN Sales s ON s.Id = sl.SaleId
                  JOIN SellUnits su ON su.Id = sl.SellUnitId
                  JOIN Products p ON p.Id = su.ProductId
                  WHERE s.ShopId = @ShopId AND s.SaleTimeUtc >= @FromUtc AND s.SaleTimeUtc < @ToUtc
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
                decimal revenue = reader.GetDecimal(reader.GetOrdinal("Revenue"));
                decimal cost = reader.GetDecimal(reader.GetOrdinal("Cost"));
                rows.Add(new ProfitReportRow
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    UnitsSold = reader.GetDecimal(reader.GetOrdinal("UnitsSold")),
                    Revenue = revenue,
                    Cost = cost,
                    Profit = revenue - cost
                });
            }
            return rows;
        }

        // Base-currency cash total (sales minus refunds) for the period; used for the expected-cash calculation.
        public decimal GetCashTotal(int shopId, DateTime fromUtc, DateTime toUtc)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                @"SELECT ISNULL(SUM(TotalAmount), 0)
                  FROM Sales
                  WHERE ShopId = @ShopId AND PaymentMethod = 'Cash'
                    AND SaleTimeUtc >= @FromUtc AND SaleTimeUtc < @ToUtc",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@FromUtc", fromUtc);
            command.Parameters.AddWithValue("@ToUtc", toUtc);
            connection.Open();
            return Convert.ToDecimal(command.ExecuteScalar());
        }

        private static Sale MapSale(SqlDataReader reader)
        {
            return new Sale
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ShopId = reader.GetInt32(reader.GetOrdinal("ShopId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                SaleTimeUtc = reader.GetDateTime(reader.GetOrdinal("SaleTimeUtc")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                RefundOfSaleId = reader.GetInt32OrNull("RefundOfSaleId"),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                TenderCurrency = reader.GetString(reader.GetOrdinal("TenderCurrency")),
                TenderRate = reader.GetDecimal(reader.GetOrdinal("TenderRate")),
                AmountTendered = reader.GetDecimalOrNull("AmountTendered"),
                ChangeGiven = reader.GetDecimalOrNull("ChangeGiven")
            };
        }

        private static SaleLine MapSaleLine(SqlDataReader reader)
        {
            return new SaleLine
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SaleId = reader.GetInt32(reader.GetOrdinal("SaleId")),
                SellUnitId = reader.GetInt32OrNull("SellUnitId"),
                NameSnapshot = reader.GetString(reader.GetOrdinal("NameSnapshot")),
                PriceSnapshot = reader.GetDecimal(reader.GetOrdinal("PriceSnapshot")),
                Qty = reader.GetDecimal(reader.GetOrdinal("Qty")),
                LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal"))
            };
        }
    }
}
