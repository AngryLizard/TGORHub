using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend
{
    /// <summary>
    /// GEneral static utility class
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Include all virtual properties in query.
        /// </summary>
        /// <assetparam name="T"></assetparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static IQueryable<T> IncludeAll<T>(this IQueryable<T> queryable) where T : EntityModel
        {
            var asset = typeof(T);
            var properties = asset.GetProperties();
            foreach (var property in properties)
            {
                var isVirtual = property?.GetGetMethod()?.IsVirtual;
                if (isVirtual == true)
                {
                    queryable = queryable.Include(property!.Name);
                }
            }
            return queryable;
        }

        /// <summary>
        /// Find entity asset by ID and include all virtual properties.
        /// </summary>
        /// <assetparam name="T"></assetparam>
        /// <param name="queryable"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T? FindInclude<T>(this IQueryable<T> queryable, long id) where T : EntityModel
        {
            return queryable.IncludeAll().FirstOrDefault(e => e.Id == id);
        }

        /// <summary>
        /// Find entity asset by ID and include all virtual properties.
        /// </summary>
        /// <assetparam name="T"></assetparam>
        /// <param name="queryable"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<T?> FindIncludeAsync<T>(this IQueryable<T> queryable, long id) where T : EntityModel
        {
            return await queryable.IncludeAll().FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Find user by ID and include all properties and subproperties needed to determine access restrictions.
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<UserModel?> FindUserForAccessAsync(this IQueryable<UserModel> queryable, long id)
        {
           return await queryable
                .Include(u => u.ProjectAccess)
                .Include(u => u.AssetAccess)
                .Include(u => u.GroupAccess).ThenInclude(g => g.ProjectAccess)
                .Include(u => u.GroupAccess).ThenInclude(g => g.AssetAccess)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Find project by ID and include all properties and subproperties needed to determine access restrictions.
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<ProjectModel?> FindProjectForAccessAsync(this IQueryable<ProjectModel> queryable, long id)
        {
            return await queryable
                 .Include(p => p.Owner)
                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Find group by ID and include all properties and subproperties needed to determine access restrictions.
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<GroupModel?> FindGroupForAccessAsync(this IQueryable<GroupModel> queryable, long id)
        {
            return await queryable
                 .Include(g => g.ProjectAccess)
                 .Include(g => g.AssetAccess)
                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Check via ID whether an entity list contains another entity.
        /// This is needed as objects with the same ID could be stored at different memory locations.
        /// </summary>
        /// <assetparam name="T"></assetparam>
        /// <param name="collection"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Includes<T>(this ICollection<T> collection, EntityModel entity) where T : EntityModel
        {
            return collection.Any(e => e.Id == entity.Id);
        }
    }
}
