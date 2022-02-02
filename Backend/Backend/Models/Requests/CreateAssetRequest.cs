namespace Backend.Models.Requests
{
    /// <summary>
    /// Request used for asset creation
    /// </summary>
    public class CreateAssetRequest : CreateEntityRequest
    {
        /// <summary>
        /// Creator of this asset for branding purposes
        /// </summary>
        public long CreatorId { get; set; } = 0;

        /// <summary>
        /// File path for this asset
        /// </summary>
        public string Url { get; set; } = "";

        /// <summary>
        /// Loader parameters
        /// </summary>
        public string Params { get; set; } = "";
    }
}
