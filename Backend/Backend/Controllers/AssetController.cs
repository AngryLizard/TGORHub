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
    /// Controller for asset management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetController : BackendController
    {
        private readonly ApplicationContext _context;

        ///
        public AssetController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get asset.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{assetId}")]
        public async Task<ActionResult<AssetModel>> GetAsset(long assetId)
        {
            AssetModel? asset = await _context.Assets.FindIncludeAsync(assetId);
            if (asset == null)
            {
                return NotFound();
            }
            return asset;
        }

        /// <summary>
        /// Get all assets.
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpGet("Admin")]
        public async Task<ActionResult<IEnumerable<AssetModel>>> AdminAssets()
        {
            return await _context.Assets.ToListAsync();
        }

        /// <summary>
        /// Get assets that are usable by the calling user.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("List/{categoryId}")]
        public async Task<ActionResult<IEnumerable<AssetModel>>> GetAssets(long categoryId)
        {
            CategoryModel? category = await _context.Categories.FindAsync(categoryId);
            if(category == null)
            {
                return NotFound("Category not found");
            }

            long userId = GetUserId();
            UserModel? user = await _context.Users.FindUserForAccessAsync(userId);
            if (user == null)
            {
                IEnumerable<AssetModel> list = await _context.Assets
                    .Include(a => a.Creator)
                    .Include(a => a.Category)
                    .Where(a => a.Category.Id == category.Id && a.Visibility == VisibilityType.Public)
                    .ToListAsync();
                return Ok(list);
            }
            else
            {
                // TODO: Make this a proper query for better performance
                IEnumerable<AssetModel> list = await _context.Assets
                    .Include(a => a.Creator)
                    .Include(a => a.Category)
                    .ToListAsync();
                return Ok(list.Where(a => HasAssetAccess(a, user)));
            }
        }

        /// <summary>
        /// Create new asset
        /// </summary>
        [Permissions(PermissionType.Content)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost("{categoryId}")]
        public async Task<ActionResult<AssetModel>> CreateAsset(long categoryId, [FromBody] CreateAssetRequest request)
        {
            UserModel? creator = await _context.Users.FindAsync(request.CreatorId);
            if (creator == null)
            {
                return NotFound("Creator invalid");
            }

            CategoryModel? category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound("category invalid");
            }

            AssetModel data = new AssetModel()
            {
                Visibility = VisibilityType.Private,
                Date = DateTime.UtcNow,
                Features = new List<FeatureModel>(),
                Name = request.Name,
                Creator = creator,
                Category = category,
                Url = request.Url,
                Params = request.Params
            };

            await _context.Assets.AddAsync(data);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateAsset", new { id = data.Id }, data);
        }

        /// <summary>
        /// Delete a asset
        /// </summary>
        [Permissions(PermissionType.Delete)]
        [HttpDelete("{assetId}")]
        public async Task<IActionResult> DeleteAsset(long assetId)
        {
            AssetModel? asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return NotFound();
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get default payload for this asset
        /// </summary>
        [AllowAnonymous]
        [HttpGet("Default/{assetId}")]
        public async Task<ActionResult<PayloadData>> GetDefault(long assetId)
        {
            AssetModel? asset = await _context.Assets.FindIncludeAsync(assetId);
            if (asset != null)
            {
                return asset.Default?.Data ?? _context.GeneratePayload(asset);
            }
            return NotFound();
        }

        /// <summary>
        /// Set asset visibility
        /// </summary>
        [Permissions(PermissionType.Public)]
        [HttpPut("Visibility/{assetId}")]
        public async Task<IActionResult> SetAssetVisibility(long assetId, [FromBody] VisibilityRequest request)
        {
            AssetModel? asset = await _context.Assets.FindIncludeAsync(assetId);
            if (asset == null)
            {
                return NotFound();
            }

            asset.Visibility = request.Visibility;
            _context.Entry(asset).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Get all features that are used by a given asset.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("Feature/{assetId}")]
        public async Task<ActionResult<IEnumerable<FeatureModel>>> GetFeatures(long assetId)
        {
            AssetModel? asset = await _context.Assets
                .Include(t => t.Features).ThenInclude(f => f.Categories)
                .FirstOrDefaultAsync(e => e.Id == assetId);
            if (asset == null)
            {
                return NotFound();
            }

            IEnumerable<FeatureModel> features = asset.Features;
            return Ok(features.OrderBy(f => f.Priority));
        }

        /// <summary>
        /// Add a feature to a asset.
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpPut("Feature/{assetId}")]
        public async Task<IActionResult> AddFeatureToAsset(long assetId, [FromBody] long featureId)
        {
            AssetModel? asset = await _context.Assets.FindIncludeAsync(assetId);
            FeatureModel? feature = await _context.Features.FindAsync(featureId);
            if (asset != null && feature != null)
            {
                if (!asset.Features.Includes(feature))
                {
                    asset.Features.Append(feature);
                    _context.Entry(asset).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Remove a feature from a asset.
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpDelete("Feature/{assetId}")]
        public async Task<IActionResult> RemoveFeatureFromAsset(long assetId, [FromBody] long featureId)
        {
            AssetModel? asset = await _context.Assets.FindIncludeAsync(assetId);
            FeatureModel? feature = await _context.Features.FindAsync(featureId);
            if (asset != null && feature != null)
            {
                if (asset.Features.Includes(feature))
                {
                    asset.Features.Append(feature);
                    _context.Entry(asset).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Add a user into a group.
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpPut("Group/{assetId}")]
        public async Task<IActionResult> AddAssetToGroup(long assetId, [FromBody] long groupId)
        {
            AssetModel? asset = await _context.Assets.FindAsync(assetId);
            GroupModel? group = await _context.Groups.FindGroupForAccessAsync(groupId);
            if (asset != null && group != null)
            {
                if (!group.AssetAccess.Includes(asset))
                {
                    group.AssetAccess.Append(asset);
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
        [Permissions(PermissionType.Content)]
        [HttpDelete("Group/{assetId}")]
        public async Task<IActionResult> RemoveAssetFromGroup(long assetId, [FromBody] long groupId)
        {
            AssetModel? asset = await _context.Assets.FindAsync(assetId);
            GroupModel? group = await _context.Groups.FindGroupForAccessAsync(groupId);
            if (asset != null && group != null)
            {
                if (group.AssetAccess.Includes(asset))
                {
                    group.AssetAccess.Append(asset);
                    _context.Entry(group).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Download asset
        /// </summary>
        [AllowAnonymous]
        [HttpGet("Download/{assetId}")]
        public async Task<ActionResult> DownloadFile(long assetId)
        {
            AssetModel? asset = await _context.Assets.FindIncludeAsync(assetId);
            if (asset != null)
            {
                byte[] urlBytes = System.IO.File.ReadAllBytes(asset.Url);
                string url = $"{asset.Name}{asset.Id}{Path.GetExtension(asset.Url)}";
                return File(urlBytes, System.Net.Mime.MediaTypeNames.Application.Octet, url);
            }
            return Unauthorized();
        }
    }
}
