using AnseNouveau.API.Auth;
using AnseNouveau.API.Dtos;
using AnseNouveau.API.Mapping;
using AnseNouveau.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnseNouveau.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("delivery")]
        public IActionResult RecordDelivery(DeliveryWriteDto deliveryWriteDto)
        {
            _stockService.RecordDelivery(deliveryWriteDto.ProductId, deliveryWriteDto.Qty,
                User.GetUserId(), deliveryWriteDto.Reason);
            return NoContent();
        }

        [HttpPost("adjustment")]
        public IActionResult RecordAdjustment(AdjustmentWriteDto adjustmentWriteDto)
        {
            _stockService.RecordAdjustment(adjustmentWriteDto.ProductId, adjustmentWriteDto.QtyDelta,
                adjustmentWriteDto.Reason, User.GetUserId());
            return NoContent();
        }

        [HttpGet("movements/{productId}")]
        public IActionResult GetMovements(int productId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return Ok(_stockService.GetMovements(productId, from, to).Select(DtoMapper.ToDto).ToList());
        }
    }
}
