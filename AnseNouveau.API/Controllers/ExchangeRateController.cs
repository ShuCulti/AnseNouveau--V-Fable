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
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public ExchangeRateController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_exchangeRateService.GetByShop(User.GetShopId()).Select(DtoMapper.ToDto).ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(ExchangeRateWriteDto exchangeRateWriteDto)
        {
            int newId = _exchangeRateService.Create(new ExchangeRate
            {
                ShopId = User.GetShopId(),
                CurrencyCode = exchangeRateWriteDto.CurrencyCode,
                RateToBase = exchangeRateWriteDto.RateToBase
            });
            return CreatedAtAction(nameof(GetAll), new { id = newId }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, ExchangeRateWriteDto exchangeRateWriteDto)
        {
            bool updated = _exchangeRateService.Update(new ExchangeRate
            {
                Id = id,
                CurrencyCode = exchangeRateWriteDto.CurrencyCode,
                RateToBase = exchangeRateWriteDto.RateToBase
            });
            return updated ? NoContent() : NotFound();
        }
    }
}
