using Backend.Models.Assets;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// Groups provide access to assets or projects to included users.
    /// </summary>
    public class GroupModel : EntityModel
    {
        /// <summary>
        /// Assets this group has access to
        /// </summary>
        [JsonConverter(typeof(EntityListJsonConverter<AssetModel>))]
        public virtual ICollection<AssetModel> AssetAccess { get; set; } = null!;

        /// <summary>
        /// Projects this group has access to
        /// </summary>
        [JsonConverter(typeof(EntityListJsonConverter<ProjectModel>))]
        public virtual ICollection<ProjectModel> ProjectAccess { get; set; } = null!;
        
        /// <summary>
        /// Navigation property.
        /// Users that belong to this group.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<UserModel> UserAccess { get; set; } = null!;

    }
}
