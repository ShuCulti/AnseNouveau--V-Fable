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
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_departmentService.GetByShop(User.GetShopId()).Select(DtoMapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Department? department = _departmentService.GetById(id);
            return department == null ? NotFound() : Ok(DtoMapper.ToDto(department));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(DepartmentWriteDto departmentWriteDto)
        {
            int newId = _departmentService.Create(new Department
            {
                ShopId = User.GetShopId(),
                Name = departmentWriteDto.Name,
                SortOrder = departmentWriteDto.SortOrder
            });
            return CreatedAtAction(nameof(GetById), new { id = newId }, null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, DepartmentWriteDto departmentWriteDto)
        {
            bool updated = _departmentService.Update(new Department
            {
                Id = id,
                Name = departmentWriteDto.Name,
                SortOrder = departmentWriteDto.SortOrder
            });
            return updated ? NoContent() : NotFound();
        }
    }
}
