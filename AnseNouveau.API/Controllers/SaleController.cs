using AnseNouveau.API.Auth;
using AnseNouveau.API.Dtos;
using AnseNouveau.API.Mapping;
using AnseNouveau.Business.Inputs;
using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnseNouveau.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SaleController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly IShopService _shopService;

        public SaleController(ISaleService saleService, IShopService shopService)
        {
            _saleService = saleService;
            _shopService = shopService;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Sale? sale = _saleService.GetById(id);
            return sale == null ? NotFound() : Ok(DtoMapper.ToDto(sale));
        }

        // Returns the receipt in the Created body so the POS can show change due immediately.
        [HttpPost]
        public IActionResult Create(SaleWriteDto saleWriteDto)
        {
            int shopId = User.GetShopId();
            List<SaleLineInput> lines = saleWriteDto.Lines.Select(l => new SaleLineInput
            {
                SellUnitId = l.SellUnitId,
                Name = l.Name,
                Price = l.Price,
                Qty = l.Qty
            }).ToList();
            Sale sale = _saleService.CompleteSale(shopId, User.GetUserId(), lines,
                saleWriteDto.PaymentMethod, saleWriteDto.TenderCurrency, saleWriteDto.AmountTendered);
            Shop shop = _shopService.GetById(shopId)!;
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, DtoMapper.ToReceiptDto(sale, shop));
        }

        [HttpPost("refund")]
        public IActionResult Refund(RefundWriteDto refundWriteDto)
        {
            Sale? refund = _saleService.RefundSale(refundWriteDto.SaleId, User.GetUserId());
            if (refund == null)
            {
                return NotFound();
            }
            return CreatedAtAction(nameof(GetById), new { id = refund.Id }, DtoMapper.ToDto(refund));
        }
    }
}
