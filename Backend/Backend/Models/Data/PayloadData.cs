
namespace Backend.Models
{
    /// <summary>
    /// Payload for one category layer.
    /// </summary>
    public class PayloadData
    {
        /// <summary>
        /// Asset used by this payload
        /// </summary>
        public long Asset { get; set; } = 0;

        /// <summary>
        /// List of customised features.
        /// </summary>
        public IEnumerable<FeatureData> Features { get; set; } = null!;
    }
}
