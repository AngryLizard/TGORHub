
using Backend.Models.Assets;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// User description used to define content access and permissions
    /// </summary>
    public class UserModel : EntityModel
    {
        /// <summary>
        /// API permissions of this user
        /// </summary>
        public PermissionType Permissions { get; set; } = PermissionType.None;

        /// <summary>
        /// Assets this user has access to
        /// </summary>
        [JsonConverter(typeof(EntityListJsonConverter<AssetModel>))]
        public virtual ICollection<AssetModel> AssetAccess { get; set; } = null!;

        /// <summary>
        /// Projects this user has access to
        /// </summary>
        [JsonConverter(typeof(EntityListJsonConverter<ProjectModel>))]
        public virtual ICollection<ProjectModel> ProjectAccess { get; set; } = null!;

        /// <summary>
        /// Groups this user is part of
        /// </summary>
        [JsonConverter(typeof(EntityListJsonConverter<GroupModel>))]
        public virtual ICollection<GroupModel> GroupAccess { get; set; } = null!;
    }
}
