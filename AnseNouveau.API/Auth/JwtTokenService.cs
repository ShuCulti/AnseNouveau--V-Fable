using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AnseNouveau.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace AnseNouveau.API.Auth
{
    public class JwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(JwtSettings settings)
        {
            _settings = settings;
        }

        public string CreateToken(AppUser user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("shopId", user.ShopId.ToString())
            };
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_settings.Key));
            SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_settings.ExpiryHours),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
