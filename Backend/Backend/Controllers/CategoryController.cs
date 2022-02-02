using Backend.Context;
using Backend.Models;
using Backend.Models.Requests;
using Backend.Models.Assets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    /// <summary>
    /// Controller for category management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BackendController
    {
        private readonly ApplicationContext _context;
        
        ///
        public CategoryController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpGet("Admin")]
        public async Task<ActionResult<IEnumerable<CategoryModel>>> AdminCategories()
        {
            return await _context.Categories
                .Include(c => c.Default)
                .ToListAsync();
        }

        /// <summary>
        /// Get category
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<CategoryModel>> GetCategory(long categoryId)
        {
            CategoryModel? category = await _context.Categories.FindIncludeAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return category;
        }


        /// <summary>
        /// Get categories for given feature
        /// </summary>
        [AllowAnonymous]
        [HttpGet("List/{featureId}")]
        public async Task<ActionResult<IEnumerable<CategoryModel>>> GetCategories(long featureId)
        {
            FeatureModel? features = await _context.Features.FindIncludeAsync(featureId);
            if (features == null)
            {
                return NotFound("Feature not found");
            }
            return Ok(features.Categories.OrderByDescending(c => c.Priority));
        }

        /// <summary>
        /// Get all root categories
        /// </summary>
        [AllowAnonymous]
        [HttpGet("Root")]
        public async Task<ActionResult<IEnumerable<CategoryModel>>> ListRootCategories()
        {
            return await _context.Categories
                .Where(c => c.Root)
                .Include(c => c.Default)
                .ToListAsync();
        }

        /// <summary>
        /// Create new category
        /// </summary>
        [Permissions(PermissionType.Content)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<CategoryModel>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            CategoryModel data = new CategoryModel()
            {
                Root = request.Root,
                Priority = request.Priority,
                Loader = request.Loader,
                Params = request.Params,
                Name = request.Name,
                Date = DateTime.UtcNow,
                Default = null
            };

            await _context.Categories.AddAsync(data);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateCategory", new { id = data.Id }, data);
        }

        /// <summary>
        /// Delete category
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(long categoryId)
        {
            CategoryModel? category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Set category default asset.
        /// Careful: This is the only way one could expose a non-public asset to the public,
        /// if the asset is public but anything trying to access this category is not.
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpPut("Default/{categoryId}")]
        public async Task<IActionResult> SetDefaultAsset(long categoryId, [FromBody] long assetId)
        {
            CategoryModel? category = await _context.Categories.FindIncludeAsync(categoryId);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            AssetModel? asset = null;
            if (assetId > 0)
            {
                asset = await _context.Assets.FindIncludeAsync(assetId);
                if (asset == null)
                {
                    return NotFound("Asset not found");
                }

                if (asset.Category.Id != categoryId)
                {
                    return BadRequest("Project isn't of the correct asset");
                }
            }

            category.Default = asset;
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
