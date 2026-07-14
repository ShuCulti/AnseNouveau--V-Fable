using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ISellUnitRepository _sellUnitRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public ProductService(
            IProductRepository productRepository,
            ISellUnitRepository sellUnitRepository,
            IStockMovementRepository stockMovementRepository)
        {
            _productRepository = productRepository;
            _sellUnitRepository = sellUnitRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        public List<Product> GetAll(int shopId)
        {
            return _productRepository.GetAll(shopId);
        }

        public Product? GetById(int id)
        {
            return _productRepository.GetById(id);
        }

        public Product? GetByBarcode(int shopId, string barcode)
        {
            return _productRepository.GetByBarcode(shopId, barcode);
        }

        public List<Product> SearchByName(int shopId, string term)
        {
            return _productRepository.SearchByName(shopId, term);
        }

        // Opening stock enters as an Adjustment movement, never as a direct StockQty write.
        public int Create(Product product, SellUnit? initialSellUnit, decimal openingQty, int userId)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ValidationException("Product name is required.");
            }
            int productId = _productRepository.Create(product);

            if (initialSellUnit != null)
            {
                initialSellUnit.ProductId = productId;
                _sellUnitRepository.Create(initialSellUnit);
            }

            if (openingQty != 0)
            {
                _stockMovementRepository.AddRange(new[]
                {
                    new StockMovement
                    {
                        ProductId = productId,
                        QtyDelta = openingQty,
                        MovementType = "Adjustment",
                        Reason = "opening stock",
                        UserId = userId,
                        MovedAt = DateTime.UtcNow
                    }
                });
            }
            return productId;
        }

        public bool Update(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ValidationException("Product name is required.");
            }
            return _productRepository.Update(product);
        }
    }
}
