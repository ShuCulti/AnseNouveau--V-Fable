using System.Security.Claims;

namespace AnseNouveau.API.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        public static int GetShopId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue("shopId")!);
        }
    }
}
