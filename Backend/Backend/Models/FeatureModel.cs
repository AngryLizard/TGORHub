using Backend.Models.Assets;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// Every feature describes a customisation step for a project.
    /// Features support layers which hold by the user per-project customisable data.
    /// </summary>
    public class FeatureModel : EntityModel
    {
        /// <summary>
        /// Features are applied from highest to lowest priority.
        /// </summary>
        [JsonIgnore]
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Frontend application used by this feature.
        /// </summary>
        public string App { get; set; } = "";

        /// <summary>
        /// Parameters to the frontend application in json format.
        /// </summary>
        public string Params { get; set; } = "";

        /// <summary>
        /// Minimum amount of layers
        /// </summary>
        public int MinLayers { get; set; } = 0;

        /// <summary>
        /// Maximum amount of layers
        /// </summary>
        public int MaxLayers { get; set; } = 0;

        /// <summary>
        /// Asset categories that need to be supplied per feature layer.
        /// These are not returned with the feature but have to be retreived 
        /// by a separate query where they're ordered.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<CategoryModel> Categories { get; set; } = null!;

        /// <summary>
        /// Float numbers that need to be supplied per feature layer.
        /// </summary>
        public int Floats { get; set; } = 1;

        /// <summary>
        /// Integer numbers that need to be supplied per feature layer.
        /// Might also be used for flags/booleans/rgba.
        /// </summary>
        public int Integers { get; set; } = 1;

        /// <summary>
        /// Navigation property.
        /// Assets supported by this feature.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<AssetModel> OwningAssets { get; set; } = null!;
    }
}
