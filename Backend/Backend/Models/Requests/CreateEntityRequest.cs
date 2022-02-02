namespace Backend.Models.Requests
{
    /// <summary>
    /// Request used for all entity creation
    /// </summary>
    public abstract class CreateEntityRequest
    {
        /// <summary>
        /// Desired display name
        /// </summary>
        public string Name { get; set; } = "";
    }
}
