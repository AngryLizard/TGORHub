using Backend.Context;
using Backend.Models;
using Backend.Models.Requests;
using Backend.Models.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Controllers
{
    /// <summary>
    /// Controller for project management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BackendController
    {
        private readonly ApplicationContext _context;

        ///
        public ProjectController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get project if accessible by the current user
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{projectId}")]
        public async Task<ActionResult<ProjectModel>> GetProject(long projectId)
        {
            long userId = GetUserId();
            UserModel? user = await _context.Users.FindUserForAccessAsync(userId);
            ProjectModel? project = await _context.Projects.FindProjectForAccessAsync(projectId);
            if (HasProjectAccess(project, user))
            {
                return Ok(project);
            }
            return Unauthorized("No project access");
        }

        /// <summary>
        /// Get all projects
        /// </summary>
        [Permissions(PermissionType.Admin)]
        [HttpGet("Admin")]
        public async Task<ActionResult<IEnumerable<ProjectModel>>> AdminAssets()
        {
            return await _context.Projects.ToListAsync();
        }

        /// <summary>
        /// Get all projects visible for calling user.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<ProjectModel>>> GetVisibleProjects()
        {
            long userId = GetUserId();
            UserModel? user = await _context.Users.FindUserForAccessAsync(userId);
            if (user == null)
            {
                return await _context.Projects
                    .Where(p => p.Visibility == VisibilityType.Public && !p.Suspended)
                    .IncludeAll().ToListAsync();
            }
            else
            {
                // TODO: Make this a proper query for better performance
                IEnumerable<ProjectModel> projects = await _context.Projects
                    .IncludeAll()
                    .OrderByDescending(p => p.Owner.Id == userId)
                    .ToListAsync();
                return Ok(projects.Where(p => HasProjectAccess(p, user)));
            }
        }

        /// <summary>
        /// Get all projects owned by calling user.
        /// These projects can be modified by the user.
        /// </summary>
        [Permissions(PermissionType.User)]
        [HttpGet("Edit")]
        public async Task<ActionResult<IEnumerable<ProjectModel>>> GetEditableProjects()
        {
            long userId = GetUserId();
            UserModel? user = await _context.Users.FindUserForAccessAsync(userId);
            // TODO: Make this a proper query for better performance
            IEnumerable<ProjectModel> projects = await _context.Projects.IncludeAll().ToListAsync();
            return Ok(projects.Where(p => CanEditProject(p, user)));
        }

        /// <summary>
        /// Create new project of a given asset for calling user
        /// </summary>
        [Permissions(PermissionType.User)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost("{categoryId}")]
        public async Task<ActionResult<ProjectModel>> CreateProject(long categoryId, [FromBody] CreateProjectRequest request)
        {
            long userId = GetUserId();
            UserModel? user = await _context.Users.FindUserForAccessAsync(userId);
            CategoryModel? category = await _context.Categories.FindIncludeAsync(categoryId);
            if (user != null && category != null)
            {
                ProjectModel project = new ProjectModel()
                {
                    Visibility = VisibilityType.Private,
                    Date = DateTime.UtcNow,
                    Suspended = false,
                    Owner = user,
                    Category = category,
                    Name = request.Name
                };

                await _context.Projects.AddAsync(project);
                await _context.SaveChangesAsync();

                return CreatedAtAction("CreateProject", new { id = project.Id }, project);
            }
            return Unauthorized();
        }

        /// <summary>
        /// Get latest payload for a given project
        /// </summary>
        [AllowAnonymous]
        [HttpGet("Payload/{projectId}")]
        public async Task<ActionResult<PayloadData>> GetPayload(long projectId)
        {
            long userId = GetUserId();
            UserModel? user = await _context.Users.FindUserForAccessAsync(userId);
            ProjectModel? project = await _context.Projects.FindProjectForAccessAsync(projectId);
            if (HasProjectAccess(project, user))
            {
                // Try to find latest payload
                PayloadModel? payload = (await _context.Payloads
                    .Include(p => p.Project)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefaultAsync(p => p.Project.Id == projectId));

                if(payload != null)
                {
                    return Ok(payload.Data);
                }
                return NotFound();
            }
            return Unauthorized();
        }

        /// <summary>
        /// Save new payload for a given project
        /// </summary>
        [Permissions(PermissionType.User)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPut("Payload/{projectId}")]
        public async Task<ActionResult<ProjectModel>> SavePayload(long projectId, [FromBody] CreatePayloadRequest request)
        {
            long userId = GetUserId();
            UserModel? user = await _context.Users.FindUserForAccessAsync(userId);
            ProjectModel? project = await _context.Projects.FindProjectForAccessAsync(projectId);
            if (project != null && project.Owner.Id == userId)
            {
                try
                {
                    // Add payload to database
                    PayloadModel payload = new PayloadModel()
                    {
                        Name = request.Name,
                        Date = DateTime.UtcNow,
                        Data = await CheckPayload(user, request.Data, null),
                        Project = project
                    };

                    await _context.Payloads.AddAsync(payload);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("CreatePayload", new { id = payload.Id }, payload);
                }
                catch (Exception e)
                {
                    BadRequest($"Bad payload: {e.Message}");
                }
            }
            return Unauthorized();
        }

        private async Task<PayloadData> CheckPayload(UserModel? user, PayloadData data, CategoryModel? category)
        {
            AssetModel? asset = await _context.Assets.FindAsync(data.Asset);
            if(asset == null)
            {
                if (category?.Default != null)
                {
                    throw new Exception($"Asset {data.Asset} requires an asset of type {category.Name} but none was given");
                }
                return data;
            }

            if (!HasAssetAccess(asset, user))
            {
                throw new Exception($"Asset {data.Asset} not found or not accessible by user {user?.Name ?? "anon"}");
            }

            if (category != null && asset != null && asset.Category != category)
            {
                throw new Exception($"Asset {data.Asset} doesn't match the expected category {category.Name}");
            }

            if (data.Features.Count() != asset!.Features.Count())
            {
                throw new Exception($"Expected {asset.Features.Count()} features but got {data.Features.Count()}");
            }

            foreach (var tuple in data.Features.Zip(asset.Features))
            {
                if (tuple.First.Layers.Count() < tuple.Second.MinLayers)
                {
                    throw new Exception($"Expected at least {tuple.Second.MinLayers} layers but got {tuple.First.Layers.Count()}");
                }
                if (tuple.First.Layers.Count() > tuple.Second.MinLayers)
                {
                    throw new Exception($"Expected at most {tuple.Second.MaxLayers} layers but got {tuple.First.Layers.Count()}");
                }

                foreach (LayerData layer in tuple.First.Layers)
                {
                    if (layer.Floats.Count() != tuple.Second.Floats)
                    {
                        throw new Exception($"Expected {tuple.Second.Floats} floats in layer but got {layer.Floats.Count()}");
                    }
                    if (layer.Integers.Count() != tuple.Second.Integers)
                    {
                        throw new Exception($"Expected {tuple.Second.Integers} integers in layer but got {layer.Integers.Count()}");
                    }
                    if (layer.Assets.Count() != tuple.Second.Categories.Count())
                    {
                        throw new Exception($"Expected {tuple.Second.Categories.Count()} assets in layer but got {layer.Assets.Count()}");
                    }

                    // Check all contained assets
                    IEnumerable<CategoryModel> categories = tuple.Second.Categories.OrderByDescending(c => c.Priority);
                    foreach ((CategoryModel c, PayloadData a) in categories.Zip(layer.Assets))
                    {
                        await CheckPayload(user, a, c);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Suspend project (we usually don't delete projects, just make them invisible)
        /// </summary>
        [Permissions(PermissionType.User)]
        [HttpPut("Suspend/{projectId}")]
        public async Task<IActionResult> SuspendProject(long projectId)
        {
            ProjectModel? project = await _context.Projects.FindProjectForAccessAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            if (project.Owner.Id == GetUserId())
            {
                project.Suspended = true;
                _context.Entry(project).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        /// <summary>
        /// Set project visibility
        /// </summary>
        [Permissions(PermissionType.Public)]
        [HttpPut("Visibility/{projectId}")]
        public async Task<IActionResult> SetProjectVisibility(long projectId, [FromBody] VisibilityRequest request)
        {
            ProjectModel? project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            project.Visibility = request.Visibility;
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Delete a project if calling user owns it.
        /// </summary>
        [Permissions(PermissionType.Delete)]
        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(long projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Add a user into a group.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpPut("Group/{projectId}")]
        public async Task<IActionResult> AddProjectToGroup(long projectId, [FromBody] long groupId)
        {
            ProjectModel? project = await _context.Projects.FindAsync(projectId);
            GroupModel? group = await _context.Groups.FindGroupForAccessAsync(groupId);
            if (project != null && group != null)
            {
                if (!group.ProjectAccess.Includes(project))
                {
                    group.ProjectAccess.Append(project);
                    _context.Entry(group).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Remove a user from a group.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpDelete("Group/{projectId}")]
        public async Task<IActionResult> RemoveProjectFromGroup(long projectId, [FromBody] long groupId)
        {
            ProjectModel? project = await _context.Projects.FindAsync(projectId);
            GroupModel? group = await _context.Groups.FindGroupForAccessAsync(groupId);
            if (project != null && group != null)
            {
                if (group.ProjectAccess.Includes(project))
                {
                    group.ProjectAccess.Append(project);
                    _context.Entry(group).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return NotFound();
        }

    }
}
