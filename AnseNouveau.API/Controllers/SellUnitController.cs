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
    public class SellUnitController : ControllerBase
    {
        private readonly ISellUnitService _sellUnitService;
        private readonly IPriceService _priceService;

        public SellUnitController(ISellUnitService sellUnitService, IPriceService priceService)
        {
            _sellUnitService = sellUnitService;
            _priceService = priceService;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            SellUnit? sellUnit = _sellUnitService.GetById(id);
            return sellUnit == null ? NotFound() : Ok(DtoMapper.ToDto(sellUnit));
        }

        [HttpGet("by-product/{productId}")]
        public IActionResult GetByProduct(int productId)
        {
            return Ok(_sellUnitService.GetByProduct(productId).Select(DtoMapper.ToDto).ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(SellUnitWriteDto sellUnitWriteDto)
        {
            int newId = _sellUnitService.Create(new SellUnit
            {
                ProductId = sellUnitWriteDto.ProductId,
                Label = sellUnitWriteDto.Label,
                Price = sellUnitWriteDto.Price,
                UnitsPerSale = sellUnitWriteDto.UnitsPerSale,
                IsCold = sellUnitWriteDto.IsCold,
                IsActive = sellUnitWriteDto.IsActive,
                SortOrder = sellUnitWriteDto.SortOrder
            });
            return CreatedAtAction(nameof(GetById), new { id = newId }, null);
        }

        // Label/flags only; the price column is changed exclusively through the price endpoint.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, SellUnitWriteDto sellUnitWriteDto)
        {
            bool updated = _sellUnitService.Update(new SellUnit
            {
                Id = id,
                ProductId = sellUnitWriteDto.ProductId,
                Label = sellUnitWriteDto.Label,
                Price = sellUnitWriteDto.Price,
                UnitsPerSale = sellUnitWriteDto.UnitsPerSale,
                IsCold = sellUnitWriteDto.IsCold,
                IsActive = sellUnitWriteDto.IsActive,
                SortOrder = sellUnitWriteDto.SortOrder
            });
            return updated ? NoContent() : NotFound();
        }

        [HttpPut("{id}/price")]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangePrice(int id, PriceChangeDto priceChangeDto)
        {
            bool updated = _priceService.ChangePrice(id, priceChangeDto.NewPrice, User.GetUserId());
            return updated ? NoContent() : NotFound();
        }

        [HttpGet("{id}/price-history")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetPriceHistory(int id)
        {
            return Ok(_priceService.GetHistory(id).Select(DtoMapper.ToDto).ToList());
        }
    }
}
