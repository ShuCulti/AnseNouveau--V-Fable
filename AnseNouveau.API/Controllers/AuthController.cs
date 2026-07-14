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
    public class AuthController : ControllerBase
    {
        // Single-counter deployment: the UI always works against shop 1.
        private const int DefaultShopId = 1;

        private readonly IAuthService _authService;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(IAuthService authService, JwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequestDto request)
        {
            AppUser? user = _authService.Login(DefaultShopId, request.UserId, request.DisplayName, request.Pin);
            if (user == null)
            {
                return Unauthorized(new { error = "Wrong PIN." });
            }
            return Ok(new LoginResponseDto
            {
                Token = _jwtTokenService.CreateToken(user),
                UserId = user.Id,
                DisplayName = user.DisplayName,
                Role = user.Role,
                ShopId = user.ShopId
            });
        }

        // The login screen shows one big button per user, so this list is anonymous by design.
        [HttpGet("users")]
        [AllowAnonymous]
        public IActionResult GetUsers()
        {
            List<AppUserDto> users = _authService.GetActiveUsers(DefaultShopId)
                .Select(DtoMapper.ToDto)
                .ToList();
            return Ok(users);
        }
    }
}
