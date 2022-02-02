using Backend.Models.Assets;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// Project description
    /// </summary>
    public class ProjectModel : EntityModel
    {
        /// <summary>
        /// Whether this project is accessible to everyone
        /// </summary>
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;

        /// <summary>
        /// Projects are suspended instead of deleted to avoid losing user data.
        /// </summary>
        public bool Suspended { get; set; } = false;

        /// <summary>
        /// Category this project was spawned from.
        /// </summary>
        [ForeignKey("CategoryId")]
        [JsonConverter(typeof(EntityJsonConverter<CategoryModel>))]
        public CategoryModel Category { get; set; } = null!;

        /// <summary>
        /// Owning user, only user that may edit this project
        /// </summary>
        [ForeignKey("OwnerId")]
        [JsonConverter(typeof(EntityJsonConverter<UserModel>))]
        public virtual UserModel Owner { get; set; } = null!;

        /// <summary>
        /// Navigation property.
        /// Users that have access to this project.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<UserModel> UserAccess { get; set; } = null!;

        /// <summary>
        /// Navigation property.
        /// Groups that have access to this project.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<GroupModel> GroupAccess { get; set; } = null!;
    }
}
