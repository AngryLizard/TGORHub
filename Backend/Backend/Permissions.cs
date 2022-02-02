using Backend.Models;
using Backend.Models.Assets;
using Microsoft.AspNetCore.Authorization;

namespace Backend
{
    /// <summary>
    /// Permission attribute to define what users have permission to call certain API scopes.
    /// </summary>
    public class Permissions : AuthorizeAttribute
    {
        /// 
        public Permissions(PermissionType role)
            : base(role.ToString())
        {
        }
    }
}