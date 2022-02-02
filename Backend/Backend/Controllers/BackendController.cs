using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Backend.Models;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using Backend.Models.Assets;

namespace Backend.Controllers
{
    /// <summary>
    /// Base class for API controllers.
    /// Contains a bunch of helpful helper functions to do with user access.
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BackendController : ControllerBase
    {
        /// <summary>
        /// Get user id associated with current token
        /// </summary>
        protected long GetUserId()
        {
            Claim? claim = User.Claims.SingleOrDefault(c => c.Type == AccountController.UserClaim);
            if (claim != null)
            {
                return long.Parse(claim.Value);
            }
            return -1;
        }

        /// <summary>
        /// Whether given user has access to given asset, 
        /// which is the case if any of the following is true:
        /// 1) Asset is public
        /// 2) User has access
        /// 3) Any group the user is part of has access
        /// </summary>
        /// <param name="asset">asset to check access to</param>
        /// <param name="user">user to check access for</param>
        /// <returns>Whether user has access to the asset</returns>
        protected bool HasAssetAccess([NotNullWhen(true)] AssetModel? asset, [NotNullWhen(true)] UserModel? user)
        {
            if (asset == null)
            { return false; }

            if (asset.Visibility == VisibilityType.Public)
            { return true; }

            return user != null &&
                    (user.AssetAccess.Includes(asset) ||
                    user.GroupAccess.Any(g => g.AssetAccess.Includes(asset)));
        }

        /// <summary>
        /// Whether given user has access to given project, 
        /// which is the case if any of the following is true:
        /// 1) Project is public
        /// 2) User has access
        /// 3) User owns the project
        /// 3) Any group the user is part of has access
        /// </summary>
        /// <param name="project">project to check access to</param>
        /// <param name="user">user to check access for</param>
        /// <returns>Whether user has access to the project</returns>
        protected bool HasProjectAccess([NotNullWhen(true)] ProjectModel? project, UserModel? user)
        {
            if (project == null || project.Suspended)
            { return false; }

            if (project.Visibility == VisibilityType.Public)
            { return true; }

            return user != null &&
                    (project.Owner.Id == user.Id ||
                    user.ProjectAccess.Includes(project) ||
                    user.GroupAccess.Any(g => g.ProjectAccess.Includes(project)));
        }

        /// <summary>
        /// Whether given user has write access to given project, 
        /// which is the case if the user owns a project and
        /// has access to the project asset.
        /// </summary>
        /// <param name="project">project to check access to</param>
        /// <param name="user">user to check access for</param>
        /// <returns>Whether user has access to the project</returns>
        protected bool CanEditProject([NotNullWhen(true)] ProjectModel? project, UserModel? user)
        {
            if (project == null || project.Suspended)
            { return false; }

            return user != null && project.Owner.Id == user.Id;
        }
    }
}
