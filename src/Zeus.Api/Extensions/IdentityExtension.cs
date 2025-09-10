using System.Security.Claims;
using System.Security.Principal;

namespace Zeus.Api.Extensions
{
    public static class IdentityExtensions
    {
        public static string? GetName(this IIdentity identity)
        {
            if (identity == null) return null;

            var claim = ((ClaimsIdentity)identity).FindFirst("Name");
            return claim?.Value ?? null;
        }

        public static string? GetUserId(this IIdentity identity)
        {
            if (identity == null) return null;

            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value ?? null;
        }

        public static string? GetUserEmail(this IIdentity identity)
        {
            if (identity == null) return null;

            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.Email)?.Value;
            if(string.IsNullOrEmpty(claim) || claim == "unknown" )
                return ((ClaimsIdentity)identity).FindFirst("signInName")?.Value;

            return claim;
        }

        public static string? GetFirstName(this IIdentity identity)
        {
            if (identity == null) return null;

            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.GivenName);
            return claim?.Value ?? null;
        }

        public static string? GetLastName(this IIdentity identity)
        {
            if (identity == null) return null;

            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.Surname);
            return claim?.Value ?? null;
        }
    }
}
