using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserNAme(this ClaimsPrincipal user){
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            return username;
        }
        public static string GetUserId(this ClaimsPrincipal user){
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId;
        }
    }
}