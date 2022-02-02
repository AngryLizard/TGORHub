namespace Backend.Models.Requests
{
    /// <summary>
    /// Request used for category creation
    /// </summary>
    public class CreateCategoryRequest : CreateEntityRequest
    {
        /// <summary>
        /// Whether this category can be used to create new projects
        /// </summary>
        public bool Root { get; set; } = false;

        /// <summary>
        /// Higher priority categories get listed first
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Loader identifier
        /// </summary>
        public string Loader { get; set; } = "";

        /// <summary>
        /// Loader parameters
        /// </summary>
        public string Params { get; set; } = "";
    }
}
