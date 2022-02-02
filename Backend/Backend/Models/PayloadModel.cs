using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// Payload containing user customisation of a whole project
    /// </summary>
    public class PayloadModel : EntityModel
    {
        /// <summary>
        /// Project this payload belongs to
        /// </summary>
        [ForeignKey("ProjectId")]
        [JsonConverter(typeof(EntityJsonConverter<ProjectModel>))]
        public virtual ProjectModel Project { get; set; } = null!;

        /// <summary>
        /// Customised data. This is configured to be converted into a json string
        /// So the ordering of all lists is maintained.
        /// TODO: May later be stored in a key-value store instead for better efficiency.
        /// </summary>
        public virtual PayloadData Data { get; set; } = null!;
    }
}
