using Backend.Context;
using Backend.Models;
using Backend.Models.Requests;
using Backend.Models.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    /// <summary>
    /// Controller for group management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : BackendController
    {
        private readonly ApplicationContext _context;

        ///
        public GroupController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get group information if available to current user
        /// </summary>
        [Authorize]
        [HttpGet("{groupId}")]
        public async Task<ActionResult<GroupModel>> GetGroup(long groupId)
        {
            long userId = GetUserId();
            UserModel? user = await _context.Users.Include(u => u.GroupAccess).FirstAsync(u => u.Id == userId);
            GroupModel? group = await _context.Groups.FindAsync(groupId);
            if(group != null && user != null && user.GroupAccess.Includes(group))
            {
                return group;
            }
            return Unauthorized();
        }

        /// <summary>
        /// Get a list of all groups.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpGet("Admin")]
        public async Task<ActionResult<IEnumerable<GroupModel>>> AdminGroups()
        {
            return await _context.Groups.IncludeAll().ToListAsync();
        }

        /// <summary>
        /// Create new group
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<GroupModel>> CreateGroup([FromBody] CreateGroupRequest request)
        {
            GroupModel data = new GroupModel()
            {
                ProjectAccess = new List<ProjectModel>(),
                AssetAccess = new List<AssetModel>(),
                Name = request.Name,
                Date = DateTime.UtcNow
            };

            await _context.Groups.AddAsync(data);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateGroup", new { id = data.Id }, data);
        }

        /// <summary>
        /// Delete a group
        /// </summary>
        [Permissions(PermissionType.Delete)]
        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(long groupId)
        {
            GroupModel? group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
