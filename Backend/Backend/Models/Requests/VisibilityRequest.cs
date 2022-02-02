using Backend.Models.Assets;

namespace Backend.Models.Requests
{
    /// <summary>
    /// Request to set an object to public or private
    /// </summary>
    public class VisibilityRequest
    {
        /// <summary>
        /// Whether to set the object to public or private
        /// </summary>
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
    }
}
