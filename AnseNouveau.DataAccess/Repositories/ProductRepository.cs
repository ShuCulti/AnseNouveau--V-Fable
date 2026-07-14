using AnseNouveau.DataAccess.Extensions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AnseNouveau.DataAccess.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private const string ProductWithSellUnitsSelect =
            @"SELECT p.Id, p.ShopId, p.DepartmentId, p.Barcode, p.Name, p.StockQty, p.CostPrice, p.IsActive, p.CreatedAt,
                     su.Id AS SellUnitId, su.Label AS SellUnitLabel, su.Price AS SellUnitPrice,
                     su.UnitsPerSale AS SellUnitUnitsPerSale, su.IsCold AS SellUnitIsCold,
                     su.IsActive AS SellUnitIsActive, su.SortOrder AS SellUnitSortOrder
              FROM Products p
              LEFT JOIN SellUnits su ON su.ProductId = p.Id AND su.IsActive = 1";

        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        public List<Product> GetAll(int shopId)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                ProductWithSellUnitsSelect +
                " WHERE p.ShopId = @ShopId AND p.IsActive = 1 ORDER BY p.Name, su.SortOrder",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return MapProductsWithSellUnits(reader);
        }

        public Product? GetById(int id)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                ProductWithSellUnitsSelect + " WHERE p.Id = @Id ORDER BY su.SortOrder",
                connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            List<Product> products = MapProductsWithSellUnits(reader);
            return products.Count > 0 ? products[0] : null;
        }

        // The hottest path in the system: one round trip returns the product and its active sell units.
        public Product? GetByBarcode(int shopId, string barcode)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                ProductWithSellUnitsSelect +
                " WHERE p.ShopId = @ShopId AND p.Barcode = @Barcode AND p.IsActive = 1 ORDER BY su.SortOrder",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@Barcode", barcode);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            List<Product> products = MapProductsWithSellUnits(reader);
            return products.Count > 0 ? products[0] : null;
        }

        public List<Product> SearchByName(int shopId, string term)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(
                ProductWithSellUnitsSelect +
                " WHERE p.ShopId = @ShopId AND p.IsActive = 1 AND p.Name LIKE @Term ORDER BY p.Name, su.SortOrder",
                connection);
            command.Parameters.AddWithValue("@ShopId", shopId);
            command.Parameters.AddWithValue("@Term", "%" + term + "%");
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            return MapProductsWithSellUnits(reader);
        }

        public int Create(Product product)
        {
            try
            {
                // StockQty is never written here: it defaults to 0 and only changes together with StockMovements.
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"INSERT INTO Products (ShopId, DepartmentId, Barcode, Name, CostPrice, IsActive)
                      VALUES (@ShopId, @DepartmentId, @Barcode, @Name, @CostPrice, @IsActive);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection);
                command.Parameters.AddWithValue("@ShopId", product.ShopId);
                command.Parameters.AddWithValue("@DepartmentId", (object?)product.DepartmentId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Barcode", (object?)product.Barcode ?? DBNull.Value);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@CostPrice", product.CostPrice);
                command.Parameters.AddWithValue("@IsActive", product.IsActive);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        public bool Update(Product product)
        {
            try
            {
                // StockQty is deliberately not updatable here; use stock movements.
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(
                    @"UPDATE Products
                      SET DepartmentId = @DepartmentId, Barcode = @Barcode, Name = @Name,
                          CostPrice = @CostPrice, IsActive = @IsActive
                      WHERE Id = @Id",
                    connection);
                command.Parameters.AddWithValue("@Id", product.Id);
                command.Parameters.AddWithValue("@DepartmentId", (object?)product.DepartmentId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Barcode", (object?)product.Barcode ?? DBNull.Value);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@CostPrice", product.CostPrice);
                command.Parameters.AddWithValue("@IsActive", product.IsActive);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (SqlException exception)
            {
                throw SqlExceptionTranslator.Translate(exception);
            }
        }

        private static List<Product> MapProductsWithSellUnits(SqlDataReader reader)
        {
            List<Product> products = new();
            Dictionary<int, Product> productsById = new();
            while (reader.Read())
            {
                int productId = reader.GetInt32(reader.GetOrdinal("Id"));
                if (!productsById.TryGetValue(productId, out Product? product))
                {
                    product = MapProduct(reader);
                    productsById.Add(productId, product);
                    products.Add(product);
                }
                int? sellUnitId = reader.GetInt32OrNull("SellUnitId");
                if (sellUnitId.HasValue)
                {
                    product.SellUnits.Add(MapJoinedSellUnit(reader, productId, sellUnitId.Value));
                }
            }
            return products;
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ShopId = reader.GetInt32(reader.GetOrdinal("ShopId")),
                DepartmentId = reader.GetInt32OrNull("DepartmentId"),
                Barcode = reader.GetStringOrNull("Barcode"),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                StockQty = reader.GetDecimal(reader.GetOrdinal("StockQty")),
                CostPrice = reader.GetDecimal(reader.GetOrdinal("CostPrice")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private static SellUnit MapJoinedSellUnit(SqlDataReader reader, int productId, int sellUnitId)
        {
            return new SellUnit
            {
                Id = sellUnitId,
                ProductId = productId,
                Label = reader.GetString(reader.GetOrdinal("SellUnitLabel")),
                Price = reader.GetDecimal(reader.GetOrdinal("SellUnitPrice")),
                UnitsPerSale = reader.GetDecimal(reader.GetOrdinal("SellUnitUnitsPerSale")),
                IsCold = reader.GetBoolean(reader.GetOrdinal("SellUnitIsCold")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("SellUnitIsActive")),
                SortOrder = reader.GetInt32(reader.GetOrdinal("SellUnitSortOrder"))
            };
        }
    }
}
