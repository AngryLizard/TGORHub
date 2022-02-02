using Backend.Models;
using Backend.Models.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Backend.Context
{
    /// <summary>
    /// Database context holding all data needed for account and content management.
    /// </summary>
    public class ApplicationContext : BackendContext
    {
        private readonly SHA256 _sha256;
        private Random _random = new Random();

        /// <summary>
        /// Customise according to extensions.
        /// </summary>
        /// <param name="options"></param>
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            _sha256 = SHA256.Create();
        }

        /// <summary>
        /// Customises database models
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store payload as string (mainly to preserve data ordering)
            modelBuilder
                .Entity<PayloadModel>()
                .Property(p => p.Data)
                .HasMaxLength(1024)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    s => JsonSerializer.Deserialize<PayloadData>(s, new JsonSerializerOptions()) ?? new PayloadData());

            modelBuilder
                .Entity<AssetModel>()
                .HasMany(a => a.Features)
                .WithMany(f => f.OwningAssets);

            modelBuilder
                .Entity<GroupModel>()
                .HasMany(u => u.AssetAccess)
                .WithMany(a => a.GroupAccess);

            modelBuilder
                .Entity<GroupModel>()
                .HasMany(u => u.ProjectAccess)
                .WithMany(p => p.GroupAccess);

            modelBuilder
                .Entity<UserModel>()
                .HasMany(u => u.AssetAccess)
                .WithMany(a => a.UserAccess);

            modelBuilder
                .Entity<UserModel>()
                .HasMany(u => u.ProjectAccess)
                .WithMany(p => p.UserAccess);

            modelBuilder
                .Entity<UserModel>()
                .HasMany(u => u.GroupAccess)
                .WithMany(g => g.UserAccess);

            modelBuilder
                .Entity<AssetModel>()
                .HasOne(u => u.Creator)
                .WithMany();

            modelBuilder
                .Entity<ProjectModel>()
                .HasOne(u => u.Owner)
                .WithMany();
        }

        /// <summary>
        /// User accounts, identifiably via their email and hashed password
        /// </summary>
        public DbSet<CredentialsModel> Users { get; set; } = null!;

        /// <summary>
        /// Groups define access rights to assets and projects.
        /// </summary>
        public DbSet<GroupModel> Groups { get; set; } = null!;

        /// <summary>
        /// Project assets that define a set of possible features.
        /// </summary>
        public DbSet<AssetModel> Assets { get; set; } = null!;

        /// <summary>
        /// Project features consisting of multiple layers.
        /// Each feature corresponds to an app page.
        /// </summary>
        public DbSet<FeatureModel> Features { get; set; } = null!;

        /// <summary>
        /// Projects contain reference a history of feature payloads.
        /// Projects can only be owned by one user who has editing rights,
        /// but multiple other people can view, download and export.
        /// </summary>
        public DbSet<ProjectModel> Projects { get; set; } = null!;

        /// <summary>
        /// Payloads contain project data for each feature and layer to build a model.
        /// Payloads are stored deparately from projects so we can load them separately 
        /// from the project description (and potentially later store them in a different key-value DB).
        /// </summary>
        public DbSet<PayloadModel> Payloads { get; set; } = null!;

        /// <summary>
        /// Categorys describe category assets that may describe species or objects to be used by project features. 
        /// E.g. textures, meshes, shaders...
        /// </summary>
        public DbSet<CategoryModel> Categories { get; set; } = null!;

        /// <summary>
        ///  Applies sha256 hashing to an input string.
        /// </summary>
        /// <param name="text">string to hash</param>
        /// <returns>string with applied sha256 hashing</returns>
        public string Hash(string text)
        {
            return BitConverter.ToString(_sha256.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
        }

        /// <summary>
        /// Generates a payload from a asset.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("asset")]
        public PayloadData? GeneratePayload(AssetModel? asset)
        {
            if(asset == null)
            {
                return null;
            }

            return new PayloadData()
            {
                Asset = asset.Id,
                Features = asset.Features
                    // Load feature data
                    ?.Select(feature => Features
                        .Include(f => f.Categories).ThenInclude(c => c.Default).ThenInclude(d => d.Features)
                        .First(f => f.Id == feature.Id))
                    // Generate layer data
                    .Select(feature => new FeatureData()
                    {
                        Layers = Enumerable.Range(0, feature.MinLayers)
                            .Select(i => new LayerData()
                            {
                                Floats = Enumerable.Repeat(0.0f, feature.Floats),
                                Integers = Enumerable.Repeat(0, feature.Integers),
                                Assets = feature.Categories.Select(c => GeneratePayload(c.Default))
                            })
                    }) ?? new List<FeatureData>()
            };
        }

        /// <summary>
        /// Check whether any table has any entries
        /// </summary>
        protected override bool HasEntries()
        {
            return
                Users.Any() ||
                Groups.Any() ||
                Assets.Any() ||
                Features.Any() ||
                Payloads.Any() ||
                Projects.Any() ||
                Categories.Any();
        }

        /// <summary>
        /// Initialise a system user and some dummy content.
        /// </summary>
        protected override void Seed()
        {
            CredentialsModel user = new CredentialsModel()
            {
                Email = "noreply@draconity.gov",
                Password = Hash(Guid.NewGuid().ToString()),
                Date = DateTime.UtcNow,
                Name = "System",
                GroupAccess = new List<GroupModel>(),
                ProjectAccess = new List<ProjectModel>(),
                AssetAccess = new List<AssetModel>(),
                Permissions = PermissionType.None
            };
            Users.Add(user);
            SaveChanges();

            CategoryModel baseCategory = new CategoryModel()
            {
                Priority = 1,
                Date = DateTime.UtcNow,
                Name = "Character",
                Loader = "Mesh",
                Root = true,
                Params = "",
                Default = null
            };

            CategoryModel textureCategory = new CategoryModel()
            {
                Priority = 0,
                Date = DateTime.UtcNow,
                Name = "Texture",
                Loader = "Texture",
                Root = false,
                Params = "",
                Default = null
            };
            Categories.AddRange(baseCategory, textureCategory);
            SaveChanges();

            FeatureModel textureFeature = new FeatureModel()
            {
                App = "Skin",
                Floats = 0,
                Integers = 0,
                Categories = new List<CategoryModel>() { textureCategory },
                Date = DateTime.UtcNow,
                MinLayers = 1,
                MaxLayers = 1,
                Name = "Texture",
                Params = "",
                Priority = 0
            };
            Features.Add(textureFeature);
            SaveChanges();

            FeatureModel colorFeature = new FeatureModel()
            {
                App = "Tint",
                Floats = 3,
                Integers = 0,
                Categories = new List<CategoryModel>() { },
                Date = DateTime.UtcNow,
                MinLayers = 3,
                MaxLayers = 3,
                Name = "Colors",
                Params = "",
                Priority = 1
            };
            Features.Add(colorFeature);
            SaveChanges();

            AssetModel foxAsset = new AssetModel()
            {
                Category = baseCategory,
                Date = DateTime.UtcNow,
                Name = "Fox",
                Url = "Resources/Gltf/fox.glb",
                Visibility = VisibilityType.Public,
                Creator = user,
                Features = new List<FeatureModel>() { colorFeature, textureFeature },
            };

            AssetModel duckAsset = new AssetModel()
            {
                Category = baseCategory,
                Date = DateTime.UtcNow,
                Name = "Duck",
                Url = "Resources/Gltf/duck.glb",
                Params = "{\"Scale\": 30}",
                Visibility = VisibilityType.Public,
                Creator = user,
                Features = new List<FeatureModel>() { colorFeature },
            };

            AssetModel avocadoAsset = new AssetModel()
            {
                Category = baseCategory,
                Date = DateTime.UtcNow,
                Name = "Avocado",
                Url = "Resources/Gltf/avocado.glb",
                Visibility = VisibilityType.Public,
                Creator = user,
                Features = new List<FeatureModel>() { },
            };

            AssetModel hognoserAsset = new AssetModel()
            {
                Category = baseCategory,
                Date = DateTime.UtcNow,
                Name = "Hognoser",
                Url = "Resources/Gltf/hognoser.glb",
                Visibility = VisibilityType.Public,
                Creator = user,
                Features = new List<FeatureModel>() { textureFeature },
            };
            Assets.AddRange(foxAsset, duckAsset, avocadoAsset, hognoserAsset);
            SaveChanges();

            baseCategory.Default = foxAsset;
            Entry(baseCategory).State = EntityState.Modified;
            SaveChanges();

            ProjectModel foxProject = new ProjectModel()
            {
                Date = DateTime.UtcNow,
                Name = "Fox",
                Owner = user,
                Suspended = false,
                Visibility = VisibilityType.Public
            };

            Projects.Add(foxProject);
            SaveChanges();

            PayloadModel foxPayload = new PayloadModel()
            {
                Name = "init",
                Date = DateTime.UtcNow,
                Data = GeneratePayload(foxAsset)!,
                Project = foxProject
            };
            Payloads.Add(foxPayload);
            SaveChangesAsync();

            GroupModel group = new GroupModel()
            {
                Date = DateTime.UtcNow,
                AssetAccess = new List<AssetModel>(),
                ProjectAccess = new List<ProjectModel>(),
                Name = "Patrons",
            };
            Groups.Add(group);
            SaveChanges();
        }
    }
}