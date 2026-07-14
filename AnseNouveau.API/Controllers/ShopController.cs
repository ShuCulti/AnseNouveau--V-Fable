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
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_shopService.GetAll().Select(DtoMapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Shop? shop = _shopService.GetById(id);
            return shop == null ? NotFound() : Ok(DtoMapper.ToDto(shop));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(ShopWriteDto shopWriteDto)
        {
            int newId = _shopService.Create(new Shop
            {
                Name = shopWriteDto.Name,
                ReceiptHeader = shopWriteDto.ReceiptHeader,
                ReceiptFooter = shopWriteDto.ReceiptFooter,
                BaseCurrency = shopWriteDto.BaseCurrency,
                TaxRatePercent = shopWriteDto.TaxRatePercent
            });
            return CreatedAtAction(nameof(GetById), new { id = newId }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, ShopWriteDto shopWriteDto)
        {
            bool updated = _shopService.Update(new Shop
            {
                Id = id,
                Name = shopWriteDto.Name,
                ReceiptHeader = shopWriteDto.ReceiptHeader,
                ReceiptFooter = shopWriteDto.ReceiptFooter,
                BaseCurrency = shopWriteDto.BaseCurrency,
                TaxRatePercent = shopWriteDto.TaxRatePercent
            });
            return updated ? NoContent() : NotFound();
        }
    }
}
