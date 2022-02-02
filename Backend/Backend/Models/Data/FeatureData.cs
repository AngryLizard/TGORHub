
namespace Backend.Models
{
    /// <summary>
    /// Payload containing user customisation of a feature
    /// </summary>
    public class FeatureData
    {
        /// <summary>
        /// List of customised layers
        /// </summary>
        public IEnumerable<LayerData> Layers { get; set; } = null!;
    }
}
