using System.Security.Claims;

namespace Wavlo
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal @this)
        {
            var claim = @this?.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value ?? string.Empty;
        }
    }
}
