using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserNAme(this ClaimsPrincipal user){
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            return username;
        }
        public static int GetUserId(this ClaimsPrincipal user){
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Convert.ToInt32(userId);
        }
    }
}