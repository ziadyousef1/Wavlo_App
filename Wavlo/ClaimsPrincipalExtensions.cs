using System.Security.Claims;

namespace Wavlo
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal @this)
        {
            return @this.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
