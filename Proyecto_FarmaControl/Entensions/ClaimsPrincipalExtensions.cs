
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Proyecto_FarmaControl.Entensions
{
    public static class ClaimsPrincipalExtensions
    {
        public const string PermissionClaimType = "Permission";

        public static bool HasPermission(
            this ClaimsPrincipal user,
            string permission)
        {
            return user.Identity?.IsAuthenticated == true &&
                user.Claims.Any(c =>
                c.Type == PermissionClaimType &&
                string.Equals(
                    c.Value,
                    permission,
                    StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasAnyPermission(
            this ClaimsPrincipal user,
            params string[] permission)
        {
            return permission.Any(user.HasPermission);
        }

        public static IReadOnlyList<string> GetPermission(
            this ClaimsPrincipal user)
        {
            return user.Claims
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .OrderBy(p => p)
                .ToList();
        }
    }
}
