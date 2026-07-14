using AnseNouveau.API.Auth;
using AnseNouveau.API.Dtos;
using AnseNouveau.API.Mapping;
using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnseNouveau.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AppUserController : ControllerBase
    {
        private readonly IAppUserService _appUserService;

        public AppUserController(IAppUserService appUserService)
        {
            _appUserService = appUserService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_appUserService.GetByShop(User.GetShopId()).Select(DtoMapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            AppUser? appUser = _appUserService.GetById(id);
            return appUser == null ? NotFound() : Ok(DtoMapper.ToDto(appUser));
        }

        [HttpPost]
        public IActionResult Create(AppUserWriteDto appUserWriteDto)
        {
            if (appUserWriteDto.Pin == null)
            {
                throw new ValidationException("New users need a PIN.");
            }
            int newId = _appUserService.Create(new AppUser
            {
                ShopId = User.GetShopId(),
                DisplayName = appUserWriteDto.DisplayName,
                Role = appUserWriteDto.Role,
                IsActive = appUserWriteDto.IsActive
            }, appUserWriteDto.Pin);
            return CreatedAtAction(nameof(GetById), new { id = newId }, null);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, AppUserWriteDto appUserWriteDto)
        {
            bool updated = _appUserService.Update(new AppUser
            {
                Id = id,
                ShopId = User.GetShopId(),
                DisplayName = appUserWriteDto.DisplayName,
                Role = appUserWriteDto.Role,
                IsActive = appUserWriteDto.IsActive
            }, appUserWriteDto.Pin);
            return updated ? NoContent() : NotFound();
        }
    }
}
