
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Proyecto_FarmaControl.Helpers;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Authorication
{
    public class JwtPermissionPolicies
    {
        public class PermissionRequirement(string permission)
            : IAuthorizationRequirement
        {
            public string Permission { get; } = permission;
        }

        public class PermissionAuthorizationHandler
            : AuthorizationHandler<PermissionRequirement>
        {
            private readonly IUsuarioRepository _users;

            public PermissionAuthorizationHandler(IUsuarioRepository users)
            {
                _users = users;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                PermissionRequirement requirement)
            {
                if (context.User.Identity?.IsAuthenticated != true ||
                    !int.TryParse(
                        context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                        out int userId) || userId <= 0)
                    return;

                // Los permisos actuales se leen de la base de datos:
                // revocaciones y concesiones se aplican inmediatamente.
                var user = await _users.GetUsuarioByIdWithPermissionAsync(
                    userId);

                if (user != null &&
                    EffectivePermission.FromUser(user).Contains(
                        requirement.Permission,
                        StringComparer.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class PermissionPolicyProvider(
            IOptions<AuthorizationOptions> options)
            : DefaultAuthorizationPolicyProvider(options)
        {
            public override async Task<AuthorizationPolicy?> GetPolicyAsync(
                string policyName)
            {
                var policy = await base.GetPolicyAsync(policyName);

                if (policy is not null)
                    return policy;

                return new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(
                        new PermissionRequirement(policyName))
                    .Build();
            }
        }
    }
}
