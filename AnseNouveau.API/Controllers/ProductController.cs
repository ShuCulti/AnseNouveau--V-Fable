using AnseNouveau.API.Auth;
using AnseNouveau.API.Dtos;
using AnseNouveau.API.Mapping;
using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnseNouveau.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_productService.GetAll(User.GetShopId()).Select(DtoMapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Product? product = _productService.GetById(id);
            return product == null ? NotFound() : Ok(DtoMapper.ToDto(product));
        }

        [HttpGet("by-barcode/{barcode}")]
        public IActionResult GetByBarcode(string barcode)
        {
            Product? product = _productService.GetByBarcode(User.GetShopId(), barcode);
            return product == null ? NotFound() : Ok(DtoMapper.ToDto(product));
        }

        [HttpGet("search")]
        public IActionResult SearchByName([FromQuery] string term)
        {
            return Ok(_productService.SearchByName(User.GetShopId(), term).Select(DtoMapper.ToDto).ToList());
        }

        [HttpPost]
        public IActionResult Create(ProductWriteDto productWriteDto)
        {
            SellUnit? initialSellUnit = null;
            if (productWriteDto.InitialSellUnit != null)
            {
                initialSellUnit = new SellUnit
                {
                    Label = productWriteDto.InitialSellUnit.Label,
                    Price = productWriteDto.InitialSellUnit.Price,
                    UnitsPerSale = productWriteDto.InitialSellUnit.UnitsPerSale,
                    IsCold = productWriteDto.InitialSellUnit.IsCold,
                    IsActive = productWriteDto.InitialSellUnit.IsActive,
                    SortOrder = productWriteDto.InitialSellUnit.SortOrder
                };
            }
            int newId = _productService.Create(new Product
            {
                ShopId = User.GetShopId(),
                DepartmentId = productWriteDto.DepartmentId,
                Barcode = productWriteDto.Barcode,
                Name = productWriteDto.Name,
                CostPrice = productWriteDto.CostPrice,
                IsActive = productWriteDto.IsActive
            }, initialSellUnit, productWriteDto.OpeningQty, User.GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = newId }, null);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, ProductWriteDto productWriteDto)
        {
            bool updated = _productService.Update(new Product
            {
                Id = id,
                DepartmentId = productWriteDto.DepartmentId,
                Barcode = productWriteDto.Barcode,
                Name = productWriteDto.Name,
                CostPrice = productWriteDto.CostPrice,
                IsActive = productWriteDto.IsActive
            });
            return updated ? NoContent() : NotFound();
        }
    }
}
