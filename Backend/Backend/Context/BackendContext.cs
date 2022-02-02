using Microsoft.EntityFrameworkCore;

namespace Backend.Context
{
    /// <summary>
    /// Base for database context classes, contains logic for seeding.
    /// </summary>
    public class BackendContext : DbContext
    {
        ///
        public BackendContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Populate the database with entries if empty.
        /// </summary>
        /// <assetparam name="Context">Context asset to seed</assetparam>
        /// <param name="scope">Scope to seed into</param>
        public static void Seed<Context>(IServiceScope scope) where Context : BackendContext
        {
            var layerContext = scope.ServiceProvider.GetService<Context>();
            if(layerContext != null)
            {
                layerContext.Database.EnsureCreated();
                if(!layerContext.HasEntries())
                {
                    layerContext.Seed();
                }
            }
        }

        /// <summary>
        /// Whether the database is already populated.
        /// </summary>
        /// <returns>True if the database has already been populated</returns>
        virtual protected bool HasEntries()
        {
            return false;
        }

        /// <summary>
        /// Inser^ts entries into this database.
        /// </summary>
        virtual protected void Seed()
        {
        }
    }
}
