using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// Base class for all identifiable objects in the application database.
    /// </summary>
    public abstract class EntityModel
    {
        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        [Key]
        public long Id { get; set; } = 0;

        /// <summary>
        /// Display name for this entity.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Timestamp for creation / last edit,
        /// </summary>
        public DateTime Date { get; set; }
    }
}
