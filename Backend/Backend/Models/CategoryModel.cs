using Backend.Models.Assets;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// Groups assets into different categories so we can define which assets are available for which project feature.
    /// Also defines file extension.
    /// </summary>
    public class CategoryModel : EntityModel
    {
        /// <summary>
        /// Whether this a root category that shows up when creating projects
        /// </summary>
        [JsonIgnore]
        public bool Root { get; set; } = false;

        /// <summary>
        /// Categories are ordered from highest to lowest priority.
        /// </summary>
        [JsonIgnore]
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Loader for this category
        /// </summary>
        public string Loader { get; set; } = "";

        /// <summary>
        /// Parameters to the loader application in json format.
        /// </summary>
        public string Params { get; set; } = "";

        /// <summary>
        /// Default asset for this category. 
        /// If none is defined this category is optional.
        /// </summary>
        [ForeignKey("DefaultId")]
        [JsonConverter(typeof(EntityJsonConverter<AssetModel>))]
        public virtual AssetModel? Default { get; set; } = null;
    }
}
