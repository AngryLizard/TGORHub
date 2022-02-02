using Backend.Models.Assets;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// Project asset defines what features are available.
    /// Access to assets can be restricted to certain users or groups.
    /// </summary>
    public class AssetModel : EntityModel
    {
        /// <summary>
        /// Author of this asset, mostly used for branding purposes
        /// </summary>
        [ForeignKey("CreatorId")]
        [JsonConverter(typeof(EntityJsonConverter<UserModel>))]
        public UserModel Creator { get; set; } = null!;

        /// <summary>
        /// Whether this asset is accessible to everyone
        /// </summary>
        public VisibilityType Visibility { get; set; } = VisibilityType.Public;

        /// <summary>
        /// Absolute path to corresponding file.
        /// Not accessibly other than through the respective asset endpoint.
        /// </summary>
        [JsonIgnore]
        public string Url { get; set; } = "";

        /// <summary>
        /// Parameters to the loader application in json format.
        /// </summary>
        public string Params { get; set; } = "";

        /// <summary>
        /// Category of this asset
        /// </summary>
        [ForeignKey("CategoryId")]
        [JsonConverter(typeof(EntityJsonConverter<CategoryModel>))]
        public virtual CategoryModel Category { get; set; } = null!;

        /// <summary>
        /// Default payload for this asset.
        /// All zero if null.
        /// </summary>
        [JsonIgnore]
        public virtual PayloadModel? Default { get; set; } = null;

        /// <summary>
        /// Feaures supported by this asset.
        /// These are not returned with the asset but have to be retreived 
        /// by a separate query where they're ordered.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<FeatureModel> Features { get; set; } = null!;

        /// <summary>
        /// Navigation property.
        /// Users that have access to this Asset.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<UserModel> UserAccess { get; set; } = null!;

        /// <summary>
        /// Navigation property.
        /// Groups that have access to this Asset.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<GroupModel> GroupAccess { get; set; } = null!;
    }
}
