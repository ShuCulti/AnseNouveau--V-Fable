using AnseNouveau.API.Auth;
using AnseNouveau.API.Mapping;
using AnseNouveau.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnseNouveau.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly IStockService _stockService;

        public ReportController(ISaleService saleService, IStockService stockService)
        {
            _saleService = saleService;
            _stockService = stockService;
        }

        [HttpGet("z")]
        public IActionResult GetZReport([FromQuery] DateTime date)
        {
            return Ok(DtoMapper.ToDto(_saleService.GetZReport(User.GetShopId(), date)));
        }

        [HttpGet("sales")]
        public IActionResult GetSales([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return Ok(_saleService.GetByDateRange(User.GetShopId(), from, to).Select(DtoMapper.ToDto).ToList());
        }

        [HttpGet("stock")]
        public IActionResult GetStockReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return Ok(_stockService.GetStockReport(User.GetShopId(), from, to).Select(DtoMapper.ToDto).ToList());
        }

        [HttpGet("stock-at-date")]
        public IActionResult GetStockAtDate([FromQuery] DateTime date)
        {
            return Ok(_stockService.GetStockAtDate(User.GetShopId(), date).Select(DtoMapper.ToDto).ToList());
        }

        [HttpGet("profit")]
        public IActionResult GetProfitReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return Ok(_stockService.GetProfitReport(User.GetShopId(), from, to).Select(DtoMapper.ToDto).ToList());
        }
    }
}
