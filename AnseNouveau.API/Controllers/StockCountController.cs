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
    public class StockCountController : ControllerBase
    {
        private readonly IStockCountService _stockCountService;

        public StockCountController(IStockCountService stockCountService)
        {
            _stockCountService = stockCountService;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            StockCount? stockCount = _stockCountService.GetById(id);
            return stockCount == null ? NotFound() : Ok(DtoMapper.ToDto(stockCount));
        }

        [HttpGet("open")]
        public IActionResult GetOpen()
        {
            StockCount? stockCount = _stockCountService.GetOpen(User.GetShopId());
            return stockCount == null ? NotFound() : Ok(DtoMapper.ToDto(stockCount));
        }

        [HttpPost]
        public IActionResult Open(StockCountWriteDto stockCountWriteDto)
        {
            int newId = _stockCountService.Open(User.GetShopId(), User.GetUserId(), stockCountWriteDto.Notes);
            return CreatedAtAction(nameof(GetById), new { id = newId }, null);
        }

        [HttpPost("{id}/lines")]
        public IActionResult SubmitLine(int id, StockCountLineWriteDto lineWriteDto)
        {
            _stockCountService.SubmitLine(id, lineWriteDto.ProductId, lineWriteDto.CountedQty);
            return NoContent();
        }

        [HttpPost("{id}/close")]
        public IActionResult Close(int id)
        {
            bool closed = _stockCountService.Close(id, User.GetUserId());
            return closed ? NoContent() : NotFound();
        }

        [HttpGet("{id}/differences")]
        public IActionResult GetDifferences(int id)
        {
            return Ok(_stockCountService.GetDifferenceReport(id).Select(DtoMapper.ToDto).ToList());
        }
    }
}
