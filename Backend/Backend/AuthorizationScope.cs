using Backend.Controllers;
using Backend.Models;
using Backend.Models.Assets;
using Microsoft.AspNetCore.Authorization;

namespace Backend
{
    /// <summary>
    /// Required permissions for a given scope
    /// </summary>
    public class ScopeRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// JWT Issuer
        /// </summary>
        public string Issuer { get; }

        /// <summary>
        /// Defined scope permission
        /// </summary>
        public PermissionType Scope { get; }

        ///
        public ScopeRequirement(string issuer, PermissionType scope)
        {
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
            Scope = scope;
        }
    }

    /// <summary>
    /// Permission handler for our scope permissions
    /// </summary>
    public class RequireScopeHandler : AuthorizationHandler<ScopeRequirement>
    {
        /// <summary>
        /// Check JWT token (claims) for current permission access
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
        {
            // The scope must have originated from our issuer.
            var scopeClaim = context.User.FindFirst(c => c.Type == AccountController.ScopeClaim && c.Issuer == requirement.Issuer);
            if (scopeClaim == null || String.IsNullOrEmpty(scopeClaim.Value))
                return Task.CompletedTask;

            // Scope mask
            if(Enum.TryParse(scopeClaim.Value, out PermissionType access) && access.HasFlag(requirement.Scope))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
