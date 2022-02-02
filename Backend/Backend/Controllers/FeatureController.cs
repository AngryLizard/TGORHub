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
    /// Controller for project features.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FeatureController : BackendController
    {
        private readonly ApplicationContext _context;

        ///
        public FeatureController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all features
        /// </summary>
        [Permissions(PermissionType.Content)]
        [HttpGet("Admin")]
        public async Task<ActionResult<IEnumerable<FeatureModel>>> AdminFeatures()
        {
            return await _context.Features.IncludeAll().ToListAsync();
        }

        /// <summary>
        /// Create new asset
        /// </summary>
        [Permissions(PermissionType.Content)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<FeatureModel>> CreateFeature([FromBody] CreateFeatureRequest request)
        {
            // Get categories from id list
            ICollection<CategoryModel?> categories = request.CategoryIds.Select(id => _context.Categories.Find(id)).ToList();
            if (categories.Any(a => a == null))
            {
                return NotFound();
            }

            FeatureModel data = new FeatureModel()
            {
                Floats = request.Floats,
                Integers = request.Integers,
                Categories = categories!,
                MinLayers = request.MinLayers,
                MaxLayers = request.MaxLayers,
                App = request.App,
                Params = request.Params,
                Priority = request.Priority,
                Name = request.Name,
                Date = DateTime.UtcNow
            };

            await _context.Features.AddAsync(data);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateFeature", new { id = data.Id }, data);
        }

        /// <summary>
        /// Delete a feature
        /// </summary>
        [Permissions(PermissionType.Delete)]
        [HttpDelete("{featureId}")]
        public async Task<IActionResult> DeleteFeature(long featureId)
        {
            FeatureModel? feature = await _context.Features.FindAsync(featureId);
            if (feature == null)
            {
                return NotFound();
            }

            _context.Features.Remove(feature);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
