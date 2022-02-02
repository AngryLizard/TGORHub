using Backend.Models.Assets;

namespace Backend.Models.Requests
{
    /// <summary>
    /// Request used for feature creation
    /// </summary>
    public class CreateFeatureRequest : CreateEntityRequest
    {
        /// <summary>
        /// Higher priority features get applied first
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Number of float parameters for each layer
        /// </summary>
        public int Floats { get; set; } = 0;

        /// <summary>
        /// Number of integer parameters for each layer
        /// </summary>
        public int Integers { get; set; } = 0;

        /// <summary>
        /// Number of category parameters for each layer
        /// </summary>
        public IEnumerable<long> CategoryIds { get; set; } = new List<long>();

        /// <summary>
        /// Minimum number of layers allowed
        /// </summary>
        public int MinLayers { get; set; } = 0;

        /// <summary>
        /// Maxiumum number of layers allowed
        /// </summary>
        public int MaxLayers { get; set; } = 0;

        /// <summary>
        /// Application identifier
        /// </summary>
        public string App { get; set; } = "";

        /// <summary>
        /// Application parameters
        /// </summary>
        public string Params { get; set; } = "";

    }
}
