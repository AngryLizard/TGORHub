
namespace Backend.Models
{
    /// <summary>
    /// Payload for one feature layer.
    /// </summary>
    public class LayerData
    {
        /// <summary>
        /// List of asset payloads
        /// </summary>
        public IEnumerable<PayloadData?> Assets { get; set; } = null!;

        /// <summary>
        /// List of float values
        /// </summary>
        public IEnumerable<float> Floats { get; set; } = null!;

        /// <summary>
        /// List of integer values
        /// </summary>
        public IEnumerable<int> Integers { get; set; } = null!;
    }
}
