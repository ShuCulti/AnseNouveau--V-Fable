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
    public class CashCountController : ControllerBase
    {
        private readonly ICashCountService _cashCountService;

        public CashCountController(ICashCountService cashCountService)
        {
            _cashCountService = cashCountService;
        }

        [HttpGet]
        public IActionResult GetByBusinessDate([FromQuery] DateTime businessDate)
        {
            CashCount? cashCount = _cashCountService.GetByBusinessDate(User.GetShopId(), businessDate);
            return cashCount == null ? NotFound() : Ok(DtoMapper.ToDto(cashCount));
        }

        [HttpGet("expected")]
        public IActionResult GetExpected([FromQuery] DateTime businessDate, [FromQuery] decimal floatAmount)
        {
            decimal expected = _cashCountService.GetExpectedCash(User.GetShopId(), businessDate, floatAmount);
            return Ok(new { expectedCash = expected });
        }

        // Returns the saved count so the difference shows immediately.
        [HttpPost]
        public IActionResult Create(CashCountWriteDto cashCountWriteDto)
        {
            CashCount cashCount = _cashCountService.SaveCount(User.GetShopId(), User.GetUserId(),
                cashCountWriteDto.BusinessDate, cashCountWriteDto.FloatAmount,
                cashCountWriteDto.CountedCash, cashCountWriteDto.Notes);
            return CreatedAtAction(nameof(GetByBusinessDate),
                new { businessDate = cashCount.BusinessDate.ToString("yyyy-MM-dd") },
                DtoMapper.ToDto(cashCount));
        }
    }
}
